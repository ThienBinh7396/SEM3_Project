using Newtonsoft.Json;
using RechargeWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace RechargeWeb.Controllers
{
    public class HomeController : Controller
    {
        public static string Base_URL = "http://localhost:49776/";
        ServerDatasDataContext data = new ServerDatasDataContext();

        [HttpPost]
        public int checkConnect()
        {
            var user = Session["user"] as customer;
            if (user == null)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        [HttpPost]
        public string UpdateBaseUrl(string url)
        {
            Base_URL = url;

            Session["baseUrl"] = url;

            ConfigurationManager.AppSettings["urlReturn"] = url + "/Home/CompletePaypal";

            Debug.WriteLine(ConfigurationManager.AppSettings["urlReturn"]);

            return url;
        }

        public ActionResult Index()
        {
            var mb = data.mobile_networks.ToList();
            var user = Session["user"] as customer;
            List<service> sv;

            if (user != null)
            {
                sv = data.services.Where(it => it.id_mobile_network == user.id_network).Take(3).ToList();
            }
            else
            {
                sv = data.services.Take(3).ToList();
            }

            var container = new ContainerData
            {
                mobile_Networks = mb,
                services = sv
            };

            return View("~/Views/Home/Index.cshtml", Tuple.Create(container));
        }
        [HttpGet]
        public ActionResult SignOut()
        {
            Session["user"] = null;
            Session["cart"] = new List<Cart>();
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public string SignUp(string name, string email, string phone, string password, string id_net_work)
        {
            var m = data.mobile_networks.Where(it => it._identity == id_net_work).ToList();
            if (m.Count() == 0)
            {
                return "faild";
            }
            var id = m[0].id;
            try
            {
                data.customers.InsertOnSubmit(new customer
                {
                    name = name,
                    email = email,
                    phone_number = phone,
                    password = password,
                    id_network = id,
                    create_at = CurrentTime().ToString(),
                    update_at = CurrentTime().ToString()
                });

                data.SubmitChanges();
                return "success";
            }
            catch (Exception e)
            {
                return "faild";
            }

        }

        [HttpPost]
        public string LoginUser(string phone, string password)
        {
            var u = data.customers
                .Where(it => it.phone_number == phone && it.password == password)
                .ToList();

            if (u.Count() == 0) return null;

            Session["user"] = u[0];

            return ToJson(new customer
            {
                name = u[0].name,
                phone_number = u[0].phone_number,
                password = u[0].password
            });

        }
        [HttpGet]
        public int AddToCart(float price, int qty, int id_service)
        {
            var user = Session["user"] as customer;
            if (user == null) return -1;

            var cart = Session["cart"] as List<Cart>;

            if (cart == null)
            {
                cart = new List<Cart>();
            }

            var check = false;

            for (var i = 0; i < cart.Count(); i++)
            {
                if (cart[i].services.id == id_service)
                {
                    cart[i].Qty += qty;
                    check = true;
                    break;
                }
            }
            if (!check)
            {
                var c = data.services.Where(it => it.id == id_service).ToList();
                if (c.Count() == 0) return 0;

                cart.Add(
                    new Cart
                    {
                        Qty = qty,
                        Price = price,
                        services = c[0]
                    });

                Session["cart"] = cart;
            }

            return 1;

        }

        [HttpPost]
        public string CheckEmail(string email)
        {
            var c = Regex.IsMatch(email, @"^[a-z].+@[a-z]+\.[a-z]+");
            if (!c)
            {
                return "type";
            }

            var check = data.customers.Where(it => it.email == email).ToList();
            if (check.Count() == 0)
            {
                return "success";
            }
            else
            {
                return "exist";
            }

        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [HttpGet]
        public string GetCarts()
        {
            var cart = Session["cart"] as List<Cart>;
            if (cart == null)
            {
                cart = new List<Cart>();
            }
            return ToJson(cart);
        }
        [HttpPost]
        public bool ControlCart(string type, int position, Cart cart)
        {
            if (Session["user"] != null)
            {
                var c = Session["cart"] as List<Cart>;
                if (type == "delete")
                {
                    if (c == null) return true;

                    c.RemoveAt(position);

                    Session["cart"] = c;
                    return true;
                }
                if (type == "update")
                {
                    if (c == null) return false;
                    if (position < c.Count)
                    {
                        c[position].Qty = cart.Qty;
                        Session["cart"] = c;
                        return true;
                    }
                }
            }
            return false;
        }
        public ActionResult ViewCheckOut()
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("NotAuthentication", "Home");
            }
            return View();
        }
        public ActionResult NotAuthentication()
        {
            return View();
        }

        public ActionResult Service()
        {
            var user = Session["user"] as customer;
            if (user != null)
            {
                var s = data.services.Where(it => it.id_mobile_network == user.id_network).ToList();
                var t = new List<type_service>();

                var check = false;
                var el = new type_service { };

                var max = 100f;

                foreach (var item in s)
                {
                    check = false;

                    el = item.type_service;

                    if (item.price > max)
                    {
                        max = float.Parse(item.price.ToString());
                    }

                    for (var i = 0; i < t.Count(); i++)
                    {
                        if (t[i].id == el.id)
                        {
                            check = true;
                            break;
                        }
                    }
                    if (!check)
                    {
                        t.Add(el);
                    }
                }

                var c = new ContainerData
                {
                    services = s,
                    type_Services = t,
                    number = max,
                    mobile_Networks = data.mobile_networks.Where(it => it.id == user.id_network).ToList()
                };

                return View(Tuple.Create(c));
            }
            return RedirectToAction("NotAuthentication", "Home");
        }

        [HttpPost]
        public int Feedback(string content)
        {
            try
            {
                var u = Session["user"] as customer;
                if (u != null)
                {
                    data.feedbacks.InsertOnSubmit(new feedback
                    {
                        content = content,
                        create_at = CurrentTime().ToString(),
                        id_from = u.id,
                        type = "receive"
                    });
                    data.SubmitChanges();
                    return 1;
                }
            }
            catch (Exception ex)
            {

            }

            return -1;
        }
        private long CurrentTime()
        {
            var Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }



        public static Dictionary<int, string> status = new Dictionary<int, string>(3)
        {
            [0] = "pending",
            [1] = "completed",
            [-1] = "reject"
        };

        [HttpPost]
        public string PreparePaypal()
        {
            if (Session["user"] != null)
            {
                var cart = Session["cart"] as List<Cart>;
                if (cart == null) cart = new List<Cart>();
                var t = 0f;

                cart.ForEach(it => t += it.Qty * it.Price);

                var code = Base64Encode(CurrentTime().ToString());
                code = code.Substring(0, code.Length - 2);

                var history = new history
                {
                    id_customer = (Session["user"] as customer).id,
                    status = status.FirstOrDefault(it => it.Value == "pending").Key,
                    payment_method = 2,
                    create_at = CurrentTime().ToString(),
                    update_at = CurrentTime().ToString(),
                    total = Math.Round(t, 2),
                    trade_code = code
                };
                data.histories.InsertOnSubmit(history);

                data.SubmitChanges();


                var list = new List<history_detail>();

                foreach (var item in cart)
                {
                    list.Add(new history_detail
                    {
                        id_history = history.id,
                        id_service = item.services.id,
                        price = item.Price,
                        qty = item.Qty
                    });
                }
                data.history_details.InsertAllOnSubmit(list);
                data.SubmitChanges();

                Session["cart"] = null;

                return history.id.ToString();
            }
            return "falid";
        }
        public ActionResult CompletePaypal(string order, string st)
        {
            var history = data.histories.Where(it => it.id == int.Parse(Base64Decode(order))).First();
            if (history != null)
            {
                history.update_at = CurrentTime().ToString();
                history.status = status.FirstOrDefault(it => it.Value == st.ToLower()).Key;

                data.SubmitChanges();

                SendEmail(history.id);
            }

            return RedirectToAction("History", "Home");
        }
        public bool SendEmail(int orderId)
        {
            var user = Session["user"] as customer;
            Mail.Send("Check your transaction", user.email, getBodyOrder(orderId));
            return true;
        }
        public int EditInformation(string name, string password)
        {
            var u = Session["user"] as customer;
            if (u != null)
            {
                try
                {
                    var user = data.customers.Where(it => it.id == u.id).Single();
                    if (user != null)
                    {
                        user.name = name;
                        user.password = password;
                        data.SubmitChanges();

                        Session["user"] = user;
                        return 1;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return -1;
        }
        public string getBodyOrder(int order)
        {
            var body = "<!DOCTYPE html><html lang='en'><head> <meta charset='utf-8'> <meta name='viewport' content='width=device-width, initial-scale=1, shrink-to-fit=no'> <title>Page Title</title></head><body> <div class='container'> <div class='row'> <div class='col'><h3>Thank you to buy service in my website.The trade code is " + data.histories.Where(it => it.id == order).Single().trade_code + ".<br> <a href='http://localhost:49776/Home/History'>Click here</a> to check your history of transaction</h3> <table class='table table-striped' style='margin-top: 36px;width: 560px;border-spacing: 0; font-family: -apple-system,BlinkMacSystemFont,Roboto,Arial,sans-serif'> <thead style='color: #fff;background-color: #212529;border-color: #32383e;'> <tr> <th style='padding: .75rem;border-bottom: 2px solid #dee2e6;'>#</th> <th style='padding: .75rem;border-bottom: 2px solid #dee2e6;'>Service name</th> <th style='padding: .75rem;border-bottom: 2px solid #dee2e6;'>Price</th> <th style='padding: .75rem;border-bottom: 2px solid #dee2e6;'>Qty</th> </tr></thead> <tbody>";

            var cart = data.history_details.Where(it => it.id_history == order).ToList();
            var i = 0;
            var t = 0f;

            foreach (var element in cart)
            {
                t += Convert.ToSingle(element.qty * element.price);
                body += "<tr> <th style='padding: .75rem;border-bottom: 2px solid #dee2e6;'> " + ++i + " </th> <td style='text-align: center;padding: .75rem;border-bottom: 2px solid #dee2e6;'> " + element.service.name + " </td><td style='text-align: center;padding: .75rem;border-bottom: 2px solid #dee2e6;'> " + Math.Round(element.price, 2) + "</td><td style='text-align: center;padding: .75rem;border-bottom: 2px solid #dee2e6;'> " + element.qty + " </td></tr>";

            }

            body += "<tr> <td colspan='2' style='padding: .75rem;'></td><td style='text-align: center;padding: .75rem;'>Total: </td><td style='text-align: center;padding: .75rem;'> $" + t + "</td></tr></tbody> </table> </div></div></div></body></html>";

            return body;
        }

        public ActionResult History()
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("NotAuthentication", "Home");
            }
            var u = Session["user"] as customer;
            var h = data.histories.Where(it => it.id_customer == u.id).OrderByDescending(it => it.id).ToList();

            var historyWithDetails = new List<HistoryWithDetails>();

            foreach (var item in h)
            {
                historyWithDetails.Add(new HistoryWithDetails
                {
                    h = item,
                    history_Details = data.history_details.Where(it => it.id_history == item.id).ToList()
                });

            }

            var container = new ContainerData
            {
                historyWithDetails = historyWithDetails
            };

            return View(Tuple.Create(container));

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

        public static string ToJson(object input)
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
    }
}