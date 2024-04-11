using ExamSystem.Comm.CommHelper;
using ExamSystem.Comm.JsonHelper;
using ExamSystem.LogicBLL.TableBLL;
using ExamSystem.Models.TableModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MVC_ExamSystem.Controllers
{
    public class ExamTextQuestionController : Controller
    {
        #region 返回json结果
        /// <summary>
        /// 返回json结果
        /// </summary>
        private readonly BsJsonResult bsJsonResult = new BsJsonResult();
        #endregion

        #region 返回试题列表视图
        /// <summary>
        /// 返回试题列表视图
        /// </summary>
        /// <returns></returns>
        public ActionResult ExamTextQuestList(int nowPage=1, int pageSize = 6)
        {
            //搜索查询方法使用通过形成的List泛型集合用兰姆达表达式进行模糊删选
            string sKeys = Request.QueryString["Keys"];
            List<Question> questions = QuestionBLL.GetExamTextQuest();
            if (!string.IsNullOrEmpty(sKeys))
                questions = questions.Where(p => p.questionName.Contains(sKeys) || p.questionList.Contains(sKeys)).ToList();
            

            int totalCount = questions.Count;
            //计算总页数
            int pageCount = Math.Max((totalCount + pageSize - 1) / pageSize, 1);

            //处理当前页为负数
            if (nowPage <= 0)
            {
                nowPage = 1;
            }
            //处理最大的页数
            if (nowPage > pageCount)
            {
                nowPage = pageCount;
            }

            ViewBag.NowSize = nowPage;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            questions = questions.OrderBy(c=>c.id).Skip(pageSize* (nowPage-1)).Take(pageSize).ToList();
            
            return View(questions);
        }
        #endregion

        #region 添加填空题视图
        /// <summary>
        /// 添加填空题视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddFillQuestion()
        {
            return View();
        }
        #endregion

        #region 添加填空题
        /// <summary>
        /// 添加填空题
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddFillQuestion(Question examTextQuest)
        {
            return Content(CheckAddFillQuestion(examTextQuest));
        }
        #endregion

        #region 验证返回值(添加填空题)
        /// <summary>
        /// 验证返回值(添加填空题)
        /// </summary>
        /// <param name="examTextQuest"></param>
        /// <returns></returns>
        private string CheckAddFillQuestion(Question examTextQuest)
        {
            string QuestionName = examTextQuest.questionName;
            string QuestionAnswer = examTextQuest.questionAnswer;
            int QuestionScore = examTextQuest.questionScore;

            if (string.IsNullOrEmpty(QuestionName) || QuestionName.Length == 0)
            {
                return bsJsonResult.ErrorResult("试题题目为空");
            }
            if (string.IsNullOrEmpty(QuestionAnswer) || QuestionAnswer.Length == 0)
            {
                return bsJsonResult.ErrorResult("试题答案为空");
            }
            if (QuestionScore <= 0 || QuestionScore.ToString() == null)
            {
                return bsJsonResult.ErrorResult("试题分数错误或者为空");
            }
            if (!QuestionBLL.AddExamFillTextQuest(examTextQuest))
            {
                return bsJsonResult.ErrorResult("试题添加失败");
            }
            return bsJsonResult.SuccessResult("试题添加成功");
        }
        #endregion

        #region 添加判断题视图
        /// <summary>
        /// 添加判断题视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddJudgeQuestion()
        {
            return View();
        }
        #endregion

        #region 添加判断题
        /// <summary>
        /// 添加判断题
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddJudgeQuestion(Question examTextQuest)
        {
            return Content(CheckAddJudgeQuestion(examTextQuest));
        }
        #endregion

        #region 验证返回值(添加判断题)
        /// <summary>
        /// 验证返回值(添加判断题)
        /// </summary>
        /// <param name="examTextQuest"></param>
        /// <returns></returns>
        private string CheckAddJudgeQuestion(Question examTextQuest)
        {
            string QuestionName = examTextQuest.questionName;
            string QuestionAnswer = examTextQuest.questionAnswer;
            int QuestionScore = examTextQuest.questionScore;

            if (string.IsNullOrEmpty(QuestionName) || QuestionName.Length == 0)
            {
                return bsJsonResult.ErrorResult("试题题目为空");
            }
            if (string.IsNullOrEmpty(QuestionAnswer) || QuestionAnswer.Length == 0)
            {
                return bsJsonResult.ErrorResult("试题答案为空");
            }
            if (QuestionScore <= 0 || QuestionScore.ToString() == null )
            {
                return bsJsonResult.ErrorResult("试题分数错误或者为空");
            }
            if (!QuestionBLL.AddExamJudgeTextQuest(examTextQuest))
            {
                return bsJsonResult.ErrorResult("试题添加失败");
            }
            return bsJsonResult.SuccessResult("试题添加成功");
        }
        #endregion

        #region 添加单选试题视图
        /// <summary>
        /// 添加单选试题视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddSingleQuestion()
        {
            return View();
        }
        #endregion

        #region 添加单选试题
        /// <summary>
        /// 添加单选试题
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddSingleQuestion(Question examTextQuest)
        {
            return Content(CheckAddSingleQuestion(examTextQuest));
        }
        #endregion

        #region 验证返回值(添加试题)
        /// <summary>
        /// 验证返回值(添加试题)
        /// </summary>
        /// <param name="examTextQuest"></param>
        /// <returns></returns>
        private string CheckAddSingleQuestion(Question examTextQuest)
        {
            string QuestionName = examTextQuest.questionName;
            string QuestionList = examTextQuest.questionList;
            int QuestionScore = examTextQuest.questionScore;

            if (string.IsNullOrEmpty(QuestionName) || QuestionName.Length == 0)
            {
                return bsJsonResult.ErrorResult("试题题目为空");
            }
            if (string.IsNullOrEmpty(QuestionList) || QuestionList.Length == 0)
            {
                return bsJsonResult.ErrorResult("试题选项为空");
            }

            if (QuestionScore <= 0 || QuestionScore.ToString() == null)
            {
                return bsJsonResult.ErrorResult("试题分数错误或者为空");
            }
            if (!QuestionBLL.AddExamSingleTextQuest(examTextQuest))
            {
                return bsJsonResult.ErrorResult("试题添加失败");
            }
            return bsJsonResult.SuccessResult("试题添加成功");
        }
        #endregion

        #region 编辑试题题目视图业务逻辑（选择题）
        /// <summary>
        /// 编辑试题题目视图业务逻辑
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditQuestion(Question examTextQuest)
        {
            if (!CommDefine.IsDigital(examTextQuest.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            return Content(CheckEditQuestionInfo(examTextQuest));
        }
        #endregion

        #region 编辑题目视图（选择题）
        /// <summary>
        /// 编辑题目视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditQuestion(string id)
        {
            //判断值Id是否存在
            Question examTextQuest = QuestionBLL.GetQuestionById(Convert.ToInt32(id));
            if (examTextQuest is null)
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "查询无相关信息！！！" });
            }
            return View(examTextQuest);
        }
        #endregion

        #region 验证返回值(编辑（选择题）试题)
        /// <summary>
        /// 验证返回值(编辑试题)
        /// </summary>
        /// <param name="examTextQuest"></param>
        /// <returns></returns>
        private string CheckEditQuestionInfo(Question examTextQuest)
        {
            string QuestionName = examTextQuest.questionName;
            string QuestionList = examTextQuest.questionList;
            String QuestionSubject = examTextQuest.questionSubject;

            if (string.IsNullOrEmpty(QuestionName) || QuestionName.Length == 0)
            {
                return bsJsonResult.ErrorResult("题目为空，请重新输入");
            }
            if (string.IsNullOrEmpty(QuestionList) || QuestionList.Length == 0)
            {
                return bsJsonResult.ErrorResult("选项为空，请重新输入");
            }
            if (string.IsNullOrEmpty(QuestionSubject) || QuestionSubject.Length == 0)
            {
                return bsJsonResult.ErrorResult("所属学科为空，请重新输入");
            }

            if (!QuestionBLL.EditQuestionInfo(examTextQuest))
            {
                return bsJsonResult.ErrorResult("试题编辑失败");
            }
            return bsJsonResult.SuccessResult("试题编辑成功");
        }
        #endregion

        #region 编辑试题题目视图业务逻辑（判断题）
        /// <summary>
        /// 编辑试题题目视图业务逻辑
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditJudgeQuestion(Question examTextQuest)
        {
            if (!CommDefine.IsDigital(examTextQuest.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            return Content(CheckEditJudgeQuestionInfo(examTextQuest));
        }
        #endregion

        #region 编辑题目视图（判断题）
        /// <summary>
        /// 编辑题目视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditJudgeQuestion(string id)
        {
            //判断值Id是否存在
            Question examTextQuest = QuestionBLL.GetQuestionById(Convert.ToInt32(id));
            if (examTextQuest is null)
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "查询无相关信息！！！" });
            }
            return View(examTextQuest);
        }
        #endregion

        #region 验证返回值(编辑（判断题）试题)
        /// <summary>
        /// 验证返回值(编辑试题)
        /// </summary>
        /// <param name="examTextQuest"></param>
        /// <returns></returns>
        private string CheckEditJudgeQuestionInfo(Question examTextQuest)
        {
            string QuestionName = examTextQuest.questionName;
            String QuestionSubject = examTextQuest.questionSubject;

            if (string.IsNullOrEmpty(QuestionName) || QuestionName.Length == 0)
            {
                return bsJsonResult.ErrorResult("题目为空，请重新输入");
            }
            if (string.IsNullOrEmpty(QuestionSubject) || QuestionSubject.Length == 0)
            {
                return bsJsonResult.ErrorResult("所属学科为空，请重新输入");
            }

            if (!QuestionBLL.EditJudgeQuestionInfo(examTextQuest))
            {
                return bsJsonResult.ErrorResult("试题编辑失败");
            }
            return bsJsonResult.SuccessResult("试题编辑成功");
        }
        #endregion

        #region 编辑试题题目视图业务逻辑（填空题）
        /// <summary>
        /// 编辑试题题目视图业务逻辑
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditFillQuestion(Question examTextQuest)
        {
            if (!CommDefine.IsDigital(examTextQuest.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            return Content(CheckEditFillQuestionInfo(examTextQuest));
        }
        #endregion

        #region 编辑题目视图（填空题）
        /// <summary>
        /// 编辑题目视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditFillQuestion(string id)
        {
            //判断值Id是否存在
            Question examTextQuest = QuestionBLL.GetQuestionById(Convert.ToInt32(id));
            if (examTextQuest is null)
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "查询无相关信息！！！" });
            }
            return View(examTextQuest);
        }
        #endregion

        #region 验证返回值(编辑（填空题）试题)
        /// <summary>
        /// 验证返回值(编辑试题)
        /// </summary>
        /// <param name="examTextQuest"></param>
        /// <returns></returns>
        private string CheckEditFillQuestionInfo(Question examTextQuest)
        {
            string QuestionName = examTextQuest.questionName;
            string QuestionAnswer = examTextQuest.questionAnswer;
            String QuestionSubject = examTextQuest.questionSubject;

            if (string.IsNullOrEmpty(QuestionName) || QuestionName.Length == 0)
            {
                return bsJsonResult.ErrorResult("题目为空，请重新输入");
            }
            if (string.IsNullOrEmpty(QuestionAnswer) || QuestionAnswer.Length == 0)
            {
                return bsJsonResult.ErrorResult("题目答案为空，请重新输入");
            }
            if (string.IsNullOrEmpty(QuestionSubject) || QuestionSubject.Length == 0)
            {
                return bsJsonResult.ErrorResult("所属学科为空，请重新输入");
            }

            if (!QuestionBLL.EditJudgeQuestionInfo(examTextQuest))
            {
                return bsJsonResult.ErrorResult("试题编辑失败");
            }
            return bsJsonResult.SuccessResult("试题编辑成功");
        }
        #endregion

        #region 删除试题
        /// <summary>
        /// 删除试题
        /// </summary>
        /// <param name="examTextQuest"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DeleteQuestion(Question examTextQuest)
        {
            //对id进行验证
            if (!CommDefine.IsDigital(examTextQuest.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法" });
            }
            if (!QuestionBLL.DeleteQuestionById(examTextQuest))
            {
                return Content(bsJsonResult.ErrorResult("删除试题失败"));
            }
            return Content(bsJsonResult.SuccessResult("删除试题成功"));
        }
        #endregion

        #region 批量删除试题
        /// <summary>
        /// 批量删除试题
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MutipleDeleteQuestion(string[] ids)
        {
            int iLen = ids.Length;
            //未选中，无值
            if (ids.Length == 0)
                return Content(bsJsonResult.ErrorResult("未选中，删除失败"));
            //有值，进行校验
            for (int i = 0; i < iLen; i++)
            {
                if (!CommDefine.IsDigital(ids[i]))
                {
                    return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
                }
                else
                {
                    if (!QuestionBLL.MutipleDeleteQuestionById(Convert.ToInt32(ids[i])))
                    {
                        return Content(bsJsonResult.ErrorResult("批量删除失败"));
                    }
                }
            }
            return Content(bsJsonResult.SuccessResult("批量删除成功"));
        }
        #endregion

        #region 更新试题状态
        /// <summary>
        /// 更新试题状态
        /// </summary>
        /// <param name="examTextQuest"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UpdateStatus(Question examTextQuest)
        {
            //对id进行验证
            if (!CommDefine.IsDigital(examTextQuest.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法" });
            }
            if (!QuestionBLL.UpdateQuestionStatus(examTextQuest))
            {
                return Content(bsJsonResult.ErrorResult("状态更新失败"));
            }
            return Content(bsJsonResult.SuccessResult("状态更新成功"));
        }
        #endregion
    }
}