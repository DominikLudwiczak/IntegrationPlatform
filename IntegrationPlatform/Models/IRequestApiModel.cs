namespace IntegrationPlatform.Models;

public interface IRequestApiModel<T>
{
    T Map();
}