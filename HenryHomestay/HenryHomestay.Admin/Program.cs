using System.Windows.Forms;
using HenryHomestay.DAL;

namespace HenryHomestay.Admin;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            DbInitializer.Initialize();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Cảnh báo khởi tạo CSDL: " + ex.Message, "Thông báo CSDL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        while (true)
        {
            using var loginForm = new LoginForm();
            if (loginForm.ShowDialog() != DialogResult.OK || loginForm.LoggedInUser == null)
            {
                break;
            }

            using var mainForm = new MainAdminForm(loginForm.LoggedInUser);
            Application.Run(mainForm);

            if (!mainForm.IsLogoutRequested)
            {
                break;
            }
        }
    }
}
