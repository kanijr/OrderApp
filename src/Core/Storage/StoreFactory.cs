
using Core.Abstractions;

namespace Core.Storage;

public static class StoreFactory
{
  public static IOrderStore Create(string[] args)
  {
    bool useFile = args.Contains("--file");
    bool useCache = args.Contains("--cache");

    string dataPath = Path.Combine(
        Directory.GetCurrentDirectory(), "data", "orders.json");

    IOrderStore store = useFile
        ? new FileOrderStore(dataPath)
        : new InMemoryOrderStore(SampleData.Orders());

    if (useCache)
      store = new CachingOrderStore(store);

    return store;
  }
}
