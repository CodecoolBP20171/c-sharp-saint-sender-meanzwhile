using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SaintSender
{
    [Serializable]
    public class Mail
    {
        public string id { get; set; }
        public string date { get; set; }
        public string from { get; set; }
        public string subject { get; set; }
        public string snippet { get; set; }
        public string body { get; set; }

        public Mail()
        {

        }

        public Mail(string id, string date, string from, string subject, string snippet)
        {
            this.id = id;
            this.date = date;
            this.from = from;
            this.subject = subject;
            this.snippet = snippet;
        }

        public async void SerializeAsync()
        {
            this.body = await Logic.GetMessageAsync(this.id);
            using (Stream stream = File.Open(@"backup\" + this.id + ".dat", FileMode.Create))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, this);
            }
        }

        public static Mail Deserialize(string fileName)
        {
            Mail mail = new Mail();
            using (Stream stream = File.Open(@"backup\" + fileName, FileMode.Open))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return (Mail) binaryFormatter.Deserialize(stream);
            }
        }
    }
}
