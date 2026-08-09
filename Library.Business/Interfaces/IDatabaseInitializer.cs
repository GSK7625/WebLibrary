namespace Library.Business.Interfaces;

public interface IDatabaseInitializer
{
    Task InitializeAsync(bool forceRecreate = false);
}
