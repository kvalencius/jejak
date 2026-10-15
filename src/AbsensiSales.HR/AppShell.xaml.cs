namespace AbsensiSales.HR;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("workspace", typeof(Views.WorkspacePage));
        Routing.RegisterRoute("register-hr", typeof(Views.HrRegisterPage));
    }
}
