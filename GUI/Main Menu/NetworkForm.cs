using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HebiKaio.Core.Profiles;

namespace GUI;

public sealed class NetworkForm : Form
{
    private readonly ProfileTransferService _transfers;
    private readonly TextBox _address = new() { Text = "127.0.0.1", Width = 180 };
    private readonly NumericUpDown _port = new() { Minimum = 1024, Maximum = 65535, Value = 45835, Width = 100 };
    private readonly Label _status = new() { Text = "Not connected", AutoSize = true, MaximumSize = new Size(650, 0) };
    private readonly ListBox _pokemon = new() { Width = 520, Height = 180 };
    private readonly CancellationTokenSource _shutdown = new();
    private TcpListener _listener;

    public NetworkForm(ProfileTransferService transfers)
    {
        _transfers = transfers;
        Text = "Local Network";
        var content = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(30) };
        content.Controls.Add(new Label { Text = "CONNECT WITH ANOTHER TRAINER", AutoSize = true, Font = new Font(SystemFonts.DefaultFont.FontFamily, 15, FontStyle.Bold) });
        content.Controls.Add(new Label { Text = "On the receiving computer, choose Host. On the sending computer, enter the host's IPv4 address, choose a Pokémon, and send it. Both computers must be on the same trusted local network.", AutoSize = true, MaximumSize = new Size(650, 0), Margin = new Padding(0, 16, 0, 16) });
        content.Controls.Add(new Label { Text = "Host address", AutoSize = true }); content.Controls.Add(_address);
        content.Controls.Add(new Label { Text = "Port", AutoSize = true }); content.Controls.Add(_port);
        content.Controls.Add(new Label { Text = "Pokémon to send", AutoSize = true }); content.Controls.Add(_pokemon);
        var actions = new FlowLayoutPanel { AutoSize = true };
        actions.Controls.AddRange([AppTheme.Button("Host / Receive", async (_, _) => await HostAsync()), AppTheme.Button("Send Selected", async (_, _) => await SendAsync()), AppTheme.Button("Refresh List", (_, _) => RefreshPokemon()), AppTheme.Button("Close", (_, _) => Close())]);
        content.Controls.Add(actions); content.Controls.Add(_status);
        Controls.Add(content); RefreshPokemon(); FormClosed += (_, _) => { _shutdown.Cancel(); _listener?.Stop(); };
    }

    private void RefreshPokemon()
    {
        _pokemon.Items.Clear(); foreach (var value in _transfers.GetExportablePokemon()) _pokemon.Items.Add(new PokemonItem(value));
        if (_pokemon.Items.Count > 0) _pokemon.SelectedIndex = 0;
    }

    private async Task HostAsync()
    {
        if (_listener is not null) return;
        try
        {
            _listener = new TcpListener(IPAddress.Any, (int)_port.Value); _listener.Start();
            var local = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(value => value.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "this computer";
            _status.Text = $"Listening at {local}:{_port.Value}. Waiting for a trainer...";
            using var client = await _listener.AcceptTcpClientAsync(_shutdown.Token);
            using var reader = new StreamReader(client.GetStream(), Encoding.UTF8, false, 4096, leaveOpen: false);
            var json = await reader.ReadToEndAsync(_shutdown.Token);
            var received = _transfers.ImportPokemonText(json);
            _status.Text = $"Received {received.Nickname ?? received.SpeciesName}. It is now in storage.";
        }
        catch (OperationCanceledException) { }
        catch (Exception exception) { _status.Text = $"Receive failed: {exception.Message}"; }
        finally { _listener?.Stop(); _listener = null; }
    }

    private async Task SendAsync()
    {
        if (_pokemon.SelectedItem is not PokemonItem selected) { _status.Text = "Choose a Pokémon to send."; return; }
        try
        {
            _status.Text = "Connecting...";
            using var client = new TcpClient(); await client.ConnectAsync(_address.Text.Trim(), (int)_port.Value, _shutdown.Token);
            var bytes = Encoding.UTF8.GetBytes(_transfers.ExportPokemonText(selected.Pokemon.Id));
            await client.GetStream().WriteAsync(bytes, _shutdown.Token); client.Close();
            _status.Text = $"Sent {selected.Pokemon.Nickname ?? selected.Pokemon.SpeciesName}.";
        }
        catch (Exception exception) when (exception is SocketException or IOException or OperationCanceledException) { _status.Text = $"Send failed: {exception.Message}"; }
    }

    private sealed record PokemonItem(OwnedPokemon Pokemon) { public override string ToString() => $"{Pokemon.Nickname ?? Pokemon.SpeciesName} — {Pokemon.SpeciesName} Lv. {Pokemon.Level}"; }
}
