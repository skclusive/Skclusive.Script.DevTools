namespace Skclusive.Script.DevTools.Tests
{
    #region ITodoStore

    public interface ITodoStoreSnapshot
    {
        string Filter { set; get; }

        ITodoSnapshot[] Todos { set; get; }
    }

    public class TodoStoreSnapshot : ITodoStoreSnapshot
    {
        public string Filter { set; get; }

        public ITodoSnapshot[] Todos { set; get; }
    }

    #endregion
}
