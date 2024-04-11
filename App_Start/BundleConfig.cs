using System.Web.Optimization;

namespace MVC_ExamSystem
{
    public class BundleConfig
    {
        // 捆绑css、Scripts
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/css").Include("~/css/font.css","~/css/xadmin.css"));
            bundles.Add(new ScriptBundle("~/lib/layui").Include("~/lib/layui/layui.js"));
            bundles.Add(new ScriptBundle("~/js").Include("~/js/xadmin.js"));
            bundles.Add(new ScriptBundle("~/Scripts").Include("~/Scripts/html5.min.js", "~/Scripts/respond.min.js", "~/Scripts/jquery-3.2.1.min.js"));
            bundles.Add(new StyleBundle("~/Styl").Include("~/Styl/font.css", "~/Styl/xadmin.css","~/Styl/layui.css"));
        }
    }
}
