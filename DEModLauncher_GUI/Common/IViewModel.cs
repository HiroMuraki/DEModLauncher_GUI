namespace DEModLauncher_GUI;

internal interface IViewModel<TModel, TViewModel>
{
    TViewModel LoadFromModel(TModel model);

    TModel ConvertToModel();
}
