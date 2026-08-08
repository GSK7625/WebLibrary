namespace Library.DataAccess.Interfaces;

public interface IDatabaseInitializer
{
    Task InitializeAsync(bool forceRecreate = false);
}
