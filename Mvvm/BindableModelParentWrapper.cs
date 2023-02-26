namespace HCL_ODA_TestPAD.Mvvm;

public abstract class BindableModelParentWrapper<TModel, TParentVm>
    : BindableModelWrapper<TModel> where TModel : class, new()
{
    public BindableModelParentWrapper(TModel model = default, TParentVm parentVm = default)
    {
        Model = model;
        Parent = parentVm;
    }

    public TParentVm Parent { get; set; }
}