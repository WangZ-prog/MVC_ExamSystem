using System.Web;
using System.Web.Mvc;

namespace MVC_ExamSystem.Filter
{
    /// <summary>
    /// 利用ActionFilterAttribute实现统一权限控制:重定向到登录界面
    /// </summary>
    public class ExamUserFilterAttribute : ActionFilterAttribute
    {
        #region OnActionExecuting在方法执行前执行
        /// <summary>
        /// OnActionExecuting在方法执行前执行
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpContext.Current.Session["LoginUser"] is null)
            {
                HttpContext.Current.Response.Redirect("/");
            }
            base.OnActionExecuting(filterContext);
        }
        #endregion

        #region OnActionExecuted在方法执行后执行
        /// <summary>
        /// OnActionExecuted在方法执行后执行
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
        }
        #endregion

        #region 在结果执行前发生(在view 呈现前)
        /// <summary>
        /// 在结果执行前发生(在view 呈现前)
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);
        }
        #endregion

        #region 在结果执行后发生(在view 呈现后) 
        /// <summary>
        ///  在结果执行后发生(在view 呈现后) 
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
        } 
        #endregion
    }
}