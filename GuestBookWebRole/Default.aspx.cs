using GuestBookData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GuestBookWebRole
{
    public partial class _Default : Page
    {
        private static GuestBookDataSource ds = new GuestBookDataSource();

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}