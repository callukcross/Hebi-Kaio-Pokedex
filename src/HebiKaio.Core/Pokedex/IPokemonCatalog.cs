namespace HebiKaio.Core.Pokedex;

public interface IPokemonCatalog
{
    IReadOnlyList<PokemonSpecies> GetAll();

    PokemonSpecies? FindByNumber(int number);
}
