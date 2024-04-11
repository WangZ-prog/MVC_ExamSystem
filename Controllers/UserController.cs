using ExamSystem.Comm.CommHelper;
using ExamSystem.Comm.JsonHelper;
using ExamSystem.Comm.Security;
using ExamSystem.DataDAL.sSqlHelper;
using ExamSystem.DataDAL.TableDAL;
using ExamSystem.LogicBLL.TableBLL;
using ExamSystem.Models.TableModel;
using KS.VerifyCode;
using System;
using System.Reflection;
using System.Web.Mvc;

namespace MVC_ExamSystem.Controllers
{
    /// <summary>
    /// 用户登录控制器类
    /// </summary>
    public class UserController : Controller
    {
        #region 返回json结果
        /// <summary>
        /// 返回json结果
        /// </summary>
        private readonly BsJsonResult bsJsonResult = new BsJsonResult();
        #endregion

        #region 业务逻辑(管理员用户)
        /// <summary>
        /// 业务逻辑(管理员用户)
        /// </summary>
        private readonly AdminUserBLL adminUserBLL = new AdminUserBLL();
        #endregion

        #region 业务逻辑(老师用户)
        /// <summary>
        /// 业务逻辑(老师用户)
        /// </summary>
        private readonly TeacherUserBLL teacherUserBLL = new TeacherUserBLL();
        #endregion

        #region 业务逻辑(学生用户)
        /// <summary>
        /// 业务逻辑(学生用户)
        /// </summary>
        private readonly StudentUserBLL studentUserBLL = new StudentUserBLL();
        #endregion

        #region 反射实现两个类的对象之间相同属性的值的复制
        /// <summary>
        /// 反射实现两个类的对象之间相同属性的值的复制
        /// 适用于没有新建实体之间
        /// </summary>
        /// <typeparam name="D">返回的实体</typeparam>
        /// <typeparam name="S">数据源实体</typeparam>
        /// <param name="d">返回的实体</param>
        /// <param name="s">数据源实体</param>
        /// <returns></returns>
        public static D MapperToModel<D, S>(D d, S s)
        {
            try
            {
                var Types = s.GetType();//获得类型  
                var Typed = typeof(D);
                foreach (PropertyInfo sp in Types.GetProperties())//获得类型的属性字段  
                {
                    foreach (PropertyInfo dp in Typed.GetProperties())
                    {
                        if (dp.Name == sp.Name && dp.PropertyType == sp.PropertyType && dp.Name != "Error" && dp.Name != "Item")//判断属性名是否相同  
                        {
                            dp.SetValue(d, sp.GetValue(s, null), null);//获得s对象属性的值复制给d对象的属性  
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return d;
        } 
        #endregion

        #region Get请求登录页面
        /// <summary>
        /// Get请求登录页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        // GET: Login
        public ActionResult Login()
        {
            return View();
        }
        #endregion

        #region Post发送验证请求
        /// <summary>
        /// Post发送验证请求
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Login(AdminUser adminUser)
        {
            TeacherUser teacherUser = new TeacherUser();
            teacherUser = MapperToModel(teacherUser, adminUser);

            StudentUser studentUser = new StudentUser();
            studentUser = MapperToModel(studentUser, adminUser);

            return adminUser.identity != "0" ? (adminUser.identity == "1" ?  Content(CheckTeacherUserInfo(teacherUser))  : Content(CheckStudentUserInfo(studentUser))) : Content(CheckAdminUserInfo(adminUser));
        }
        #endregion

        #region  验证返回值(登录管理员)
        /// <summary>
        /// 验证返回值(登录)
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="imgCode"></param>
        /// <returns></returns>
        private string CheckAdminUserInfo(AdminUser adminUser)
        {
            string Username = adminUser.userName;
            string Password = adminUser.passWord;
            string Imgcode = adminUser.ImgCode;

            if (string.IsNullOrEmpty(Username) || Username.Length == 0)
            {
                return bsJsonResult.ErrorResult("请输入用户账号");
            }
            if (string.IsNullOrEmpty(Password) || Password.Length == 0)
            {
                return bsJsonResult.ErrorResult("请输入用户密码");
            }
            if (string.IsNullOrEmpty(Imgcode) || Imgcode.Length == 0)
            {
                return bsJsonResult.ErrorResult("请输入验证码");
            }
            if (Session["CheckCode"] is null)
            {
                return bsJsonResult.ErrorResult("用户不存在或密码错误，登录失败");
            }
            else if (!Session["CheckCode"].ToString().Equals(Imgcode, StringComparison.InvariantCultureIgnoreCase))
            {
                //StringComparison.InvariantCultureIgnoreCase 忽略大小写
                //return "WrongCode";
                return bsJsonResult.WrongCodeResult("验证码错误");
            }

            AdminUser successUser = adminUserBLL.GetAdminUser(adminUser);
            if (successUser is null) 
                return bsJsonResult.ErrorResult("身份不匹配或用户不存在，请重新输入");

            if (successUser.adminStatus == 0) 
                return bsJsonResult.ErrorResult("用户状态为已停用，请联系管理员");
            Session["LoginUser"] = successUser;//登录成功后的验证
            return bsJsonResult.SuccessResult("登录成功");
        }
        #endregion

        #region  验证返回值(登录老师)
        /// <summary>
        /// 验证返回值(登录老师)
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="imgCode"></param>
        /// <returns></returns>
        private string CheckTeacherUserInfo(TeacherUser teacherUser)
        {
            string Username = teacherUser.userName;
            string Password = teacherUser.passWord;
            string Imgcode = teacherUser.ImgCode;

            if (string.IsNullOrEmpty(Username) || Username.Length == 0)
            {
                return bsJsonResult.ErrorResult("请输入用户账号");
            }
            if (string.IsNullOrEmpty(Password) || Password.Length == 0)
            {
                return bsJsonResult.ErrorResult("请输入用户密码");
            }
            if (string.IsNullOrEmpty(Imgcode) || Imgcode.Length == 0)
            {
                return bsJsonResult.ErrorResult("请输入验证码");
            }
            if (Session["CheckCode"] is null)
            {
                return bsJsonResult.ErrorResult("用户不存在或密码错误，登录失败");
            }
            else if (!Session["CheckCode"].ToString().Equals(Imgcode, StringComparison.InvariantCultureIgnoreCase))
            {
                //StringComparison.InvariantCultureIgnoreCase 忽略大小写
                //return "WrongCode";
                return bsJsonResult.WrongCodeResult("验证码错误");
            }

            TeacherUser successUser = TeacherUserBLL.GetTeacherUser(teacherUser);
            if (successUser is null) 
                return bsJsonResult.ErrorResult("身份不匹配或用户不存在，请重新输入");
           
            if (successUser.teacherStatus == 0)
                return bsJsonResult.ErrorResult("用户状态为已停用，请联系管理员");
            Session["LoginUser"] = successUser;//登录成功后的验证
            return bsJsonResult.SuccessResult("登录成功");
        }
        #endregion

        #region  验证返回值(登录学生)
        /// <summary>
        /// 验证返回值(登录学生)
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="imgCode"></param>
        /// <returns></returns>
        private string CheckStudentUserInfo(StudentUser studentUser)
        {
            string Username = studentUser.userName;
            string Password = studentUser.passWord;
            string Imgcode = studentUser.ImgCode;

            if (string.IsNullOrEmpty(Username) || Username.Length == 0)
            {
                return bsJsonResult.ErrorResult("请输入用户账号");
            }
            if (string.IsNullOrEmpty(Password) || Password.Length == 0)
            {
                return bsJsonResult.ErrorResult("请输入用户密码");
            }
            if (string.IsNullOrEmpty(Imgcode) || Imgcode.Length == 0)
            {
                return bsJsonResult.ErrorResult("请输入验证码");
            }
            if (Session["CheckCode"] is null)
            {
                return bsJsonResult.ErrorResult("用户不存在或密码错误，登录失败");
            }
            else if (!Session["CheckCode"].ToString().Equals(Imgcode, StringComparison.InvariantCultureIgnoreCase))
            {
                //StringComparison.InvariantCultureIgnoreCase 忽略大小写
                //return "WrongCode";
                return bsJsonResult.WrongCodeResult("验证码错误");
            }

            StudentUser successUser = StudentUserBLL.GetStudentUser(studentUser);
            if (successUser is null) 
                return bsJsonResult.ErrorResult("身份不匹配或用户不存在，请重新输入");
           
            if (successUser.studentStatus == 0) 
                return bsJsonResult.ErrorResult("用户状态为已停用，请联系管理员");
            Session["LoginUser"] = successUser;//登录成功后的验证
            return bsJsonResult.SuccessResult("登录成功");
        }
        #endregion=

        #region  验证返回值(注册老师)
        /// <summary>
        /// 验证返回值(注册老师)
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private string CheckRegisterTeacherUserInfo(TeacherUser teacherUser)
        {
            string Username = teacherUser.userName;
            string sPassword = teacherUser.passWord;
            string sRePassword = teacherUser.repassWord;
            string Imgcode = teacherUser.ImgCode;
            string Name = teacherUser.realName;
            string Telphone = teacherUser.telPhone;


            if (string.IsNullOrEmpty(Username) || Username.Length == 0)
            {
                return bsJsonResult.ErrorResult("用户名为空，请重新输入");
            }
            TeacherUser successUser = TeacherUserBLL.ChackRegisterUserName(teacherUser);
            if (!(TeacherUserDAL.ChackRegisterUserName(teacherUser) is null))
                return bsJsonResult.ErrorResult("用户名已存在，请重新输入");
            if (string.IsNullOrEmpty(sPassword) || sPassword.Length == 0)
            {
                return bsJsonResult.ErrorResult("密码为空，请重新输入");
            }
            //验证密码
            if (string.IsNullOrEmpty(sPassword))
            {
                return bsJsonResult.ErrorResult("密码不能为空");
            }
            if (string.IsNullOrEmpty(sRePassword))
            {
                return bsJsonResult.ErrorResult("确认密码不能为空");
            }
            if (sPassword.Length < 6 || sPassword.Length > 12)
            {
                return bsJsonResult.ErrorResult("密码必须在6-12位");
            }
            if (!sPassword.Equals(sRePassword))
            {
                return bsJsonResult.ErrorResult("两次密码输入不一致");
            }

            if (string.IsNullOrEmpty(Name) || Name.Length == 0)
            {
                return bsJsonResult.ErrorResult("真实姓名不能为空");
            }
            //判断是否为汉字
            if (!CommDefine.IsChineseChar(Name))
            {
                return bsJsonResult.ErrorResult("真实姓名必须为汉字");
            }
            if (string.IsNullOrEmpty(Telphone) || Telphone.Length == 0)
            {
                return bsJsonResult.ErrorResult("电话号码不能为空");
            }
            //判断电话号码是否为11位
            if (!CommDefine.IsTelphone(Telphone))
            {
                return bsJsonResult.ErrorResult("电话号码必须为11位，且1开头");
            }
            //验证管理员手机号
            if (!(TeacherUserDAL.ChackUserTelphone(teacherUser) is null))
            {
                return bsJsonResult.ErrorResult("手机号已存在，请重新输入");
            }
            if (Session["CheckCode"] is null)
            {
                return bsJsonResult.ErrorResult("验证码为空，请重新输入");
            }
            else if (!Session["CheckCode"].ToString().Equals(Imgcode, StringComparison.InvariantCultureIgnoreCase))
            {
                //StringComparison.InvariantCultureIgnoreCase 忽略大小写
                //return "WrongCode";
                return bsJsonResult.WrongCodeResult("验证码输入错误");
            }
            if (TeacherUserBLL.AddRegisterInfo(teacherUser) is false)
                return bsJsonResult.ErrorResult("老师注册失败");
            Session["LoginUser"] = successUser;//登录成功后的验证
            return bsJsonResult.SuccessResult("老师注册成功");
        }
        #endregion

        #region  验证返回值(注册学生)
        /// <summary>
        /// 验证返回值(注册学生)
        /// </summary>
        /// <param name="studentUser"></param>
        /// <returns></returns>
        private string CheckRegisterStudentUserInfo(StudentUser studentUser)
        {
            string Username = studentUser.userName;
            string sPassword = studentUser.passWord;
            string sRePassword = studentUser.repassWord;
            string Imgcode = studentUser.ImgCode;
            string Name = studentUser.realName;
            string Telphone = studentUser.telPhone;


            if (string.IsNullOrEmpty(Username) || Username.Length == 0)
            {
                return bsJsonResult.ErrorResult("用户名为空，请重新输入");
            }
            StudentUser successUser = StudentUserBLL.ChackRegisterUserName(studentUser);
            if (!(StudentUserDAL.ChackRegisterUserName(studentUser) is null))
                return bsJsonResult.ErrorResult("用户名已存在，请重新输入");
            if (string.IsNullOrEmpty(sPassword) || sPassword.Length == 0)
            {
                return bsJsonResult.ErrorResult("密码为空，请重新输入");
            }
            //验证密码
            if (string.IsNullOrEmpty(sPassword))
            {
                return bsJsonResult.ErrorResult("密码不能为空");
            }
            if (string.IsNullOrEmpty(sRePassword))
            {
                return bsJsonResult.ErrorResult("确认密码不能为空");
            }
            if (sPassword.Length < 6 || sPassword.Length > 12)
            {
                return bsJsonResult.ErrorResult("密码必须在6-12位");
            }
            if (!sPassword.Equals(sRePassword))
            {
                return bsJsonResult.ErrorResult("两次密码输入不一致");
            }

            if (string.IsNullOrEmpty(Name) || Name.Length == 0)
            {
                return bsJsonResult.ErrorResult("真实姓名不能为空");
            }
            //判断是否为汉字
            if (!CommDefine.IsChineseChar(Name))
            {
                return bsJsonResult.ErrorResult("真实姓名必须为汉字");
            }
            if (string.IsNullOrEmpty(Telphone) || Telphone.Length == 0)
            {
                return bsJsonResult.ErrorResult("电话号码不能为空");
            }
            //判断电话号码是否为11位
            if (!CommDefine.IsTelphone(Telphone))
            {
                return bsJsonResult.ErrorResult("电话号码必须为11位，且1开头");
            }
            //验证管理员手机号
            if (!(StudentUserDAL.ChackUserTelphone(studentUser) is null))
            {
                return bsJsonResult.ErrorResult("手机号已存在，请重新输入");
            }
            if (Session["CheckCode"] is null)
            {
                return bsJsonResult.ErrorResult("验证码为空，请重新输入");
            }
            else if (!Session["CheckCode"].ToString().Equals(Imgcode, StringComparison.InvariantCultureIgnoreCase))
            {
                //StringComparison.InvariantCultureIgnoreCase 忽略大小写
                //return "WrongCode";
                return bsJsonResult.WrongCodeResult("验证码输入错误");
            }
            if (StudentUserBLL.AddRegisterInfo(studentUser) is false)
                return bsJsonResult.ErrorResult("学生注册失败");
            Session["LoginUser"] = successUser;//登录成功后的验证
            return bsJsonResult.SuccessResult("学生注册成功");
        }
        #endregion

        #region 显示验证码方法
        /// <summary>
        /// 显示验证码方法
        /// </summary>
        public void VerifyCode()
        {
            ImageVerifyCode imageVerify = new ImageVerifyCode();
            imageVerify.ValidateCode();
            string s = Session["CheckCode"].ToString();
        }
        #endregion

        #region Get请求注册页面
        /// <summary>
        /// Get请求注册页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        #endregion

        #region Post发送验证请求(注册)
        /// <summary>
        /// Post发送验证请求(注册)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Register(AdminUser adminUser)
        {
            TeacherUser teacherUser = new TeacherUser();
            StudentUser studentUser = new StudentUser();
            return adminUser.identity == "1" ? Content(CheckRegisterTeacherUserInfo(MapperToModel(teacherUser, adminUser))) : 
                Content(CheckRegisterStudentUserInfo(MapperToModel(studentUser, adminUser)));
        }
        #endregion
    }
}