namespace Skclusive.Script.DevTools.Tests
{
    #region ITodo

    public interface ITodoSnapshot
    {
        string Title { set; get; }

        bool Done { set; get; }
    }

    public class TodoSnapshot : ITodoSnapshot
    {
        public string Title { set; get; }

        public bool Done { set; get; }
    }

    #endregion
}
