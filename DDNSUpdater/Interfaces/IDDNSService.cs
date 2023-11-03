namespace DDNSUpdater.Interfaces;

public interface IDDNSService
{
    public Task Init();
    public Task Update(bool changed);
    public Task SetUpdateURL();
}