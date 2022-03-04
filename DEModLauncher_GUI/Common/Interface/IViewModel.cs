namespace DEModLauncher_GUI {
    public interface IViewModel<TModel, TViewModel> {
        TViewModel LoadFromModel(TModel model);
        TModel ConvertToModel();
    }
}
