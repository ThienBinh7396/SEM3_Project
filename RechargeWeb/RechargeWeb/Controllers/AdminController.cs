using Newtonsoft.Json;
using RechargeWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RechargeWeb.Controllers
{
    public class AdminController : Controller
    {
        ServerDatasDataContext data = new ServerDatasDataContext();
        static List<int> permissionAdmin = new List<int>() { 1 };
        static List<int> permissionAll = new List<int>() { 1, 2 };

        Dictionary<string, List<int>> permission = new Dictionary<string, List<int>>()
        {
            ["Index"] = permissionAll,
            ["Employee"] = permissionAdmin,
            ["Transaction"] = permissionAll,
            ["Customer"] = permissionAll,
            ["Service"] = permissionAll,
            ["Network"] = permissionAll

        };
        private bool CheckPermission(string page)
        {
            var admin = Session["admin"] as employee;
            if (admin != null)
            {
                if (permission[page].Contains(admin.role)) return true;

            }
            return false;
        }
        [HttpGet]
        public ActionResult Index()
        {
            if (checkLogin() && CheckPermission("Index"))
            {
                Session["transaction"] = data.histories.Where(it => it.status == HomeController.status.FirstOrDefault(item => item.Value == "completed").Key).ToList().Count();


                return View(data.histories.ToList());
            }
            else
            {
                return RedirectToAction("Login", "Admin");
            }
        }

        [HttpGet]
        public ActionResult Customer()
        {
            if (checkLogin() && CheckPermission("Customer"))
            {
                var c = data.customers.ToList();
                var s = new List<StatisticCustomerWithTransaction>();

                foreach (var item in c)
                {
                    var h = data.histories.Where(it => it.id_customer == item.id).ToList();
                    var hs = h.Where(it => it.status == 1);
                    var t = 0f;
                    h.ForEach(it => t += (float)it.total);

                    s.Add(new StatisticCustomerWithTransaction
                    {
                        customers = item,
                        amount = h.Count(),
                        amount_success = hs.Count(),
                        total = t
                    });
                }

                return View(s);
            }
            else
            {
                return RedirectToAction("Login", "Admin");
            }
        }
        [HttpGet]
        public ActionResult Profile()
        {
            if (checkLogin())
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Admin");
            }
        }
        [HttpPost]
        public ActionResult Profile(string name, string password)
        {
            if (checkLogin())
            {
                var d = Session["admin"] as employee;

                try
                {
                    var e = data.employees.Where(it => it.id == d.id).Single();

                    e.name = name;
                    e.password = password;
                    data.SubmitChanges();

                    Session["admin"] = e;
                    return View(new MessageFromServer
                    {
                        type = "success",
                        message = "Congratulation!!!. Change your profile successful"
                    });
                }
                catch (Exception ex)
                {
                    return View(new MessageFromServer
                    {
                        type = "error",
                        message = "Something went wrong, can't change your profile"
                    });
                }
            }
            else
            {
                return RedirectToAction("Login", "Admin");
            }
        }
        public ActionResult Employee()
        {
            if (checkLogin() && CheckPermission("Employee"))
            {
                var e = Session["admin"] as employee;
                return View(data.employees.Where(it => it.id != e.id).ToList());
            }
            else
            {
                return RedirectToAction("Login", "Admin");
            }
        }
        [HttpGet]
        public ActionResult Service()
        {
            if (checkLogin() && CheckPermission("Service"))
            {
                var container = new ContainerData
                {
                    services = data.services.ToList(),
                    type_Services = data.type_services.ToList(),
                    mobile_Networks = data.mobile_networks.ToList()
                };

                return View(Tuple.Create(container));
            }
            else
            {
                return RedirectToAction("Login", "Admin");
            }
        }
        [HttpPost]
        public int ControlTypeService(string type, int id, string title)
        {
            try
            {
                if (checkLogin())
                {
                    if (type == "add")
                    {
                        data.type_services.InsertOnSubmit(new type_service
                        {
                            title = title
                        });
                        data.SubmitChanges();
                        return 1;
                    }

                    var t = data.type_services.Where(it => it.id == id).Single();
                    if (type == "delete")
                    {
                        if (id != 1)
                        {
                            data.type_services.DeleteOnSubmit(t);
                            data.SubmitChanges();
                            return 1;
                        }
                    }
                    if (type == "edit")
                    {
                        if (id != 1)
                        {
                            t.title = title;
                            data.SubmitChanges();
                            return 1;
                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }

            return -1;
        }
        [HttpPost]
        [ValidateInput(false)]
        public int ControlService(string type, int id, string name, int type_service, int mobile, string start, string end, string description, float price)
        {
            try
            {
                if (checkLogin())
                {
                    if (type == "add")
                    {
                        if (start == " " && end == " ")
                        {
                            data.services.InsertOnSubmit(new service
                            {
                                name = name,
                                type_id = type_service,
                                price = price,
                                description = description,
                                id_mobile_network = mobile,
                            });
                        }
                        else
                        {
                            data.services.InsertOnSubmit(new service
                            {
                                name = name,
                                type_id = type_service,
                                price = price,
                                description = description,
                                id_mobile_network = mobile,
                                start_date = start,
                                end_date = end
                            });
                        }
                        data.SubmitChanges();
                        return 1;
                    }

                    if (type == "edit")
                    {
                        var s = data.services.Where(it => it.id == id).Single();

                        if (s != null)
                        {
                            s.name = name;
                            s.type_id = type_service;
                            s.description = description;
                            s.id_mobile_network = mobile;
                            s.start_date = start.Trim();
                            s.end_date = end.Trim();

                            data.SubmitChanges();
                            return 1;
                        }
                    }
                    if (type == "delete")
                    {
                        var s = data.services.Where(it => it.id == id).Single();
                        if (s != null)
                        {
                            data.services.DeleteOnSubmit(s);
                            data.SubmitChanges();

                            return 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return -1;
        }
        [HttpPost]
        public int DeleteService(int id)
        {
            if (checkLogin())
            {
                try
                {
                    var s = data.services.Where(it => it.id == id).Single();
                    if (s != null)
                    {
                        data.services.DeleteOnSubmit(s);
                        data.SubmitChanges();

                        return 1;
                    }

                }
                catch (Exception ex)
                {

                }
            }
            return -1;
        }
        [HttpGet]
        public ActionResult NetWork()
        {
            if (checkLogin() && CheckPermission("Network"))
            {
                var m = data.mobile_networks.ToList();

                return View(m);
            }
            else
            {
                return RedirectToAction("Login", "Admin");
            }

        }
        [HttpPost]
        public int ControlNetWork()
        {
            if (checkLogin())
            {
                try
                {
                    var name = Request.Form["name"];
                    var country = Request.Form["country"];
                    var identity = Request.Form["identity"];
                    var type = Request.Form["type"];


                    var file = Request.Files["thumbnail"];
                    var fileName = "";
                    if (file != null)
                    {
                        fileName = CurrentTime() + Path.GetFileName(file.FileName);

                        file.SaveAs(Server.MapPath("~/Assets/Upload/") + fileName);
                    }
                    if (type == "add")
                    {
                        data.mobile_networks.InsertOnSubmit(new mobile_network
                        {
                            name = name,
                            country = name,
                            _identity = identity,
                            thumbnail = fileName
                        });
                        data.SubmitChanges();
                        return 1;
                    }
                    if (type == "edit")
                    {
                        var id = int.Parse(Request.Form["id"].ToString());
                        var m = data.mobile_networks.Where(it => it.id == id).Single();
                        if (m != null)
                        {
                            m.name = name;
                            m._identity = identity;
                            m.country = country;
                            if (file != null)
                            {
                                m.thumbnail = fileName;
                            }

                            data.SubmitChanges();
                            return 1;
                        }
                    }

                }
                catch (Exception ex)
                {

                }
            }
            return -1;

        }
        [HttpPost]
        public int DeleteNetwork(int id)
        {
            if (checkLogin())
            {
                try
                {
                    var n = data.mobile_networks.Where(it => it.id == id).Single();
                    if (n != null)
                    {
                        data.mobile_networks.DeleteOnSubmit(n);
                        data.SubmitChanges();

                        return 1;
                    }
                }
                catch (Exception ex)
                {

                }


            }
            return -1;
        }
        [HttpGet]
        public ActionResult Login(int? id)
        {
            if (!checkLogin())
            {
                if (id == null)
                {
                    return View("~/Views/Admin/Login.cshtml", new employee());
                }
                else
                {
                    return View("~/Views/Admin/Login.cshtml", data.employees.Where(it => it.id == id).Single());
                }
            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }
        }
        [HttpPost]
        public string Login(string email, string password)
        {
            var e = data.employees.Where(it => it.email == email && it.password == password).ToList();

            if (e.Count() == 0)
            {
                return "false";
            }
            else
            {
                Session["admin"] = e[0];
                return "true";
            }
        }
        [HttpGet]
        public ActionResult Logout()
        {
            Session["admin"] = null;
            return RedirectToAction("Login", "Admin");
        }
        [HttpGet]
        public ActionResult Register()
        {
            if (!checkLogin())
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }
        }
        [HttpPost]
        public ActionResult Register(employee employee)
        {
            try
            {
                employee.create_at = CurrentTime().ToString();
                employee.update_at = employee.create_at;
                employee.role = 2;

                data.employees.InsertOnSubmit(employee);
                data.SubmitChanges();
                var body = "Create account successful.<br>Your account is " + employee.email + ".<br>Your password is " + employee.password + ".<br>Don't forget this password.";

                Mail.Send("Sign up successful", employee.email, body);

                return RedirectToAction("Login", "Admin", new { @id = employee.id });
            }
            catch (Exception e)
            {
                return RedirectToAction("Register", "Admin");

            }
        }
        [HttpPost]
        public bool UpdateAvatar(string background, string color)
        {
            if (checkLogin())
            {
                var a = Session["admin"] as employee;
                try
                {
                    var u = data.employees.Where(it => it.id == a.id).Single();

                    if (u != null)
                    {
                        u.background = background;
                        u.color = color;

                        data.SubmitChanges();
                        Session["admin"] = u;

                        return true;
                    }
                }
                catch (Exception e)
                {
                    return false;
                }

            }
            return false;
        }
        public ActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        public int ForgetPassword(string email)
        {
            try
            {
                var u = data.employees.Where(it => it.email == email).Single();

                var time = Base64Encode(CurrentTime().ToString());
                time = time.Substring(0, time.Length - 2);
                time = time.Substring(time.Length - 8, 8).ToLower();
                var body = "Your new password is " + time + ". <a href='" + HomeController.Base_URL + "/Admin/Login/" + u.id + "'>Click here</a> login then rechange your password";

                if (u == null)
                {
                    return 0; //email don't find
                }

                u.password = time;
                data.SubmitChanges();

                Mail.Send("Change password", email, body);
                return 1; //successful
            }
            catch (Exception e)
            {
                return 0;//error
            }
        }
        public bool checkLogin()
        {
            var admin = Session["admin"] as employee;
            if (admin == null) return false;
            return true;
        }
        public ActionResult Transaction()
        {
            if (checkLogin() && CheckPermission("Transaction"))
            {
                return View(data.histories.OrderByDescending(it => it.create_at).ToList());
            }
            else
            {
                return RedirectToAction("Login", "Admin");
            }
        }
        [HttpGet]
        public string GetDetailsTransaction(int transaction)
        {
            if (checkLogin())
            {
                return ToJson(new HistoryWithDetails
                {
                    h = data.histories.Where(it => it.id == transaction).Single(),
                    history_Details = data.history_details.Where(it => it.id_history == transaction).ToList()
                });

            }
            else
            {
                return "";
            }
        }
        [HttpGet]
        public bool ChangeStatus(int id, string status)
        {
            if (checkLogin())
            {
                try
                {
                    var h = data.histories.Where(it => it.id == id).Single();
                    if (h != null)
                    {
                        h.status = HomeController.status.FirstOrDefault(it => it.Value == status).Key;
                        h.id_employee_update = (Session["admin"] as employee).id;
                        data.SubmitChanges();
                        return true;
                    }
                }
                catch (Exception e)
                {

                }

            }
            return false;
        }

        public ActionResult Feedback()
        {
            if (checkLogin())
            {
                var feedbackWithCustomers = new List<FeedbackWithCustomer>();
                var f = data.feedbacks.Where(it => it.type == "receive").ToList();
                var check = false;

                for (var i = 0; i < f.Count; i++)
                {
                    check = false;
                    for (var j = 0; j < feedbackWithCustomers.Count; j++)
                    {
                        if (feedbackWithCustomers[j].customers.id == f[i].id_from)
                        {
                            check = true;
                            feedbackWithCustomers[j].receiveFeedbacks.Add(f[i]);
                            break;
                        }
                    }
                    if (!check)
                    {
                        feedbackWithCustomers.Add(new FeedbackWithCustomer
                        {
                            customers = data.customers.Where(it => it.id == f[i].id_from).Single(),
                            receiveFeedbacks = new List<feedback>() { f[i] }
                        });
                    }
                }

                for (var i = 0; i < feedbackWithCustomers.Count; i++)
                {
                    feedbackWithCustomers[i].sendFeedbacks =
                        data.feedbacks.Where(it => it.id_to == feedbackWithCustomers[i].customers.id && it.type == "send").ToList();


                }

                foreach (var xxx in feedbackWithCustomers)
                {
                    var fbs = xxx.receiveFeedbacks;
                    fbs.AddRange(xxx.sendFeedbacks);

                    var temp = new feedback { };
                    for (var x = 0; x < fbs.Count - 1; x++)
                    {
                        for (var y = x + 1; y < fbs.Count; y++)
                        {
                            if (long.Parse(fbs[y].create_at) < long.Parse(fbs[x].create_at))
                            {
                                temp = fbs[y];
                                fbs[y] = fbs[x];
                                fbs[x] = temp;
                            }
                        }
                    }
                    xxx.listWithSort = fbs;
                }

                return View(feedbackWithCustomers);
            }
            return RedirectToAction("Login", "Admin");
        }
        [HttpPost]
        [ValidateInput(false)]
        public int SendFeedback(string content, int id)
        {
            if (checkLogin())
            {
                try
                {
                    var e = Session["admin"] as employee;

                    if (e != null)
                    {

                        data.feedbacks.InsertOnSubmit(new feedback
                        {
                            content = content,
                            create_at = CurrentTime().ToString(),
                            id_from = e.id,
                            id_to = id,
                            type = "send",
                        });
                        data.SubmitChanges();
                        Mail.Send("Thanks your feed back", data.customers.Where(it => it.id == id).Single().email, content);

                        return 1;
                    }
                }
                catch (Exception ex)
                {

                }

            }
            return -1;
        }
        public static string Base64Decode(string plainText)
        {
            var plainTextBytes = System.Convert.FromBase64String(plainText);
            return System.Text.ASCIIEncoding.ASCII.GetString(plainTextBytes);
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private string ToJson(object input)
        {
            string json = JsonConvert.SerializeObject(
                input,
                Newtonsoft.Json.Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    PreserveReferencesHandling = PreserveReferencesHandling.Arrays
                });

            return json;
        }
        private long CurrentTime()
        {
            var Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }
    }
}