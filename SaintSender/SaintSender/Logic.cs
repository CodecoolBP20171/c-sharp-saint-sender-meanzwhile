
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Mail;

namespace SaintSender
{
    public class Logic
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/gmail-dotnet-quickstart.json
        static string[] Scopes = { GmailService.Scope.MailGoogleCom };
        static string ApplicationName = "Gmail API .NET Quickstart";

        private static GmailService Connection()
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/gmail-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Gmail API service.
            GmailService service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
            return service;
        }

        public static async Task<String> GetMessageAsync(string messageID)
        {
            var service = Connection();

            var emailInfoRequest = service.Users.Messages.Get("me", messageID);
            var emailInfoResponse = await emailInfoRequest.ExecuteAsync();

            String from = "";
            String date = "";
            String subject = "";
            String body = "";

            foreach (var mParts in emailInfoResponse.Payload.Headers)
            {
                if (mParts.Name == "Date")
                {
                    date = mParts.Value;
                }
                else if (mParts.Name == "From")
                {
                    from = mParts.Value;
                }
                else if (mParts.Name == "Subject")
                {
                    subject = mParts.Value;
                }

                if (date != "" && from != "")
                {
                    if (emailInfoResponse.Payload.Parts == null && emailInfoResponse.Payload.Body != null)
                    {
                        body = DecodeBase64String(emailInfoResponse.Payload.Body.Data);
                    }
                    else
                    {
                        body = getNestedParts(emailInfoResponse.Payload.Parts, "");
                    }
                }
            }
            if (body.Contains("<html>"))
            {
                body = body.Substring(body.IndexOf("<html>") + "<html>".Length);
            }
            return body;  
        }

        static String DecodeBase64String(string s)
        {
            var ts = s.Replace("-", "+");
            ts = ts.Replace("_", "/");
            var bc = Convert.FromBase64String(ts);
            var tts = Encoding.UTF8.GetString(bc);

            return tts;
        }

        static String getNestedParts(IList<MessagePart> part, string curr)
        {
            string str = curr;
            if (part == null)
            {
                return str;
            }
            else
            {
                foreach (var parts in part)
                {
                    if (parts.Parts == null)
                    {
                        if (parts.Body != null && parts.Body.Data != null)
                        {
                            var ts = DecodeBase64String(parts.Body.Data);
                            str += ts;
                        }
                    }
                    else
                    {
                        return getNestedParts(parts.Parts, str);
                    }
                }
                return str;
            }
        }

        public static async Task<List<Mail>> GetAllMailAsync()
        {
            var service = Connection();


            var emailListRequest = service.Users.Messages.List("me");
            emailListRequest.LabelIds = "INBOX";
            emailListRequest.IncludeSpamTrash = false;

            var emailListResponse = await emailListRequest.ExecuteAsync();

            List<Mail> allMails = new List<Mail>(); 

            if (emailListResponse != null && emailListResponse.Messages != null)
            {
                foreach (var email in emailListResponse.Messages)
                {
                    var emailInfoRequest = service.Users.Messages.Get("me", email.Id);
                    var emailInfoResponse = await emailInfoRequest.ExecuteAsync();

                    if (emailInfoResponse != null)
                    {
                        Mail mail = new Mail();

                        mail.id = email.Id;
                        mail.snippet = emailInfoResponse.Snippet;

                        foreach (var mParts in emailInfoResponse.Payload.Headers)
                        {
                            if (mParts.Name == "Date")
                            {
                                mail.date = mParts.Value;
                            }
                            else if (mParts.Name == "From")
                            {
                                mail.from = mParts.Value;
                            }
                            else if (mParts.Name == "Subject")
                            {
                                mail.subject = mParts.Value;
                            }
                        }
                        allMails.Add(mail);
                    }
                }
            }
            return allMails;
        }

        public static List<Mail> restoreAllMails()
        {
            List<Mail> restoredMails = new List<Mail>();
            DirectoryInfo d = new DirectoryInfo(@"backup");
            FileInfo[] fileInfo = d.GetFiles("*.dat");
            foreach (FileInfo file in fileInfo)
            {
                restoredMails.Add(Mail.Deserialize(file.Name));
            }

            return restoredMails;
        }

        public static void SendMail(string to, string subject, string body)
        {
            var service = Connection();
            string plainText = "To: "+ to +"\r\n" +
                "Subject: "+ subject + "\r\n" +
                "Content-Type: text/html; charset=us-ascii\r\n\r\n" +
                body;

            var newMsg = new Google.Apis.Gmail.v1.Data.Message
            {
                Raw = Base64UrlEncode(plainText.ToString())
            };
            service.Users.Messages.Send(newMsg, "me").Execute();
        }

        public static string Base64UrlEncode(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        public static void DeleteMessage(string messageID)
        {
            var service = Connection();
            service.Users.Messages.Trash("me", messageID).Execute();
        }
    }
}