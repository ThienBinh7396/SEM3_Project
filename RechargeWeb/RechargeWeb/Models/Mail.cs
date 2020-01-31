using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace RechargeWeb.Models
{
    public class Mail
    {
        public static void Send(string subject, string to, string body)
        {
            try
            {
                MailMessage Msg = new MailMessage();
                Msg.From = new MailAddress("binhhvd00682@fpt.edu.vn");
                Msg.To.Add(to);
                Msg.Subject = subject;
                Msg.Body = body;
                Msg.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                NetworkCred.UserName = "binhhvd00682@fpt.edu.vn";
                NetworkCred.Password = "binh@1996";
                smtp.UseDefaultCredentials = true;

                smtp.Credentials = NetworkCred;
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Send(Msg);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

        }
    }
}