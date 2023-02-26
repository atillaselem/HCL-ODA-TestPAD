using System;

namespace HCL_ODA_TestPAD.Mvvm;


public abstract class BindableModelWrapper<TModel> : BindableBase where TModel : class
{
    TModel _model;

    public TModel Model
    {
        get => _model;
        set => SetProperty(ref _model, value);
    }

    protected BindableModelWrapper(TModel model = default)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
    }

}
