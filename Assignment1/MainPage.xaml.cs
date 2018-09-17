using Assignment1.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Assignment1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<User> Users { get => Model.UserModel.GetUsers(); set => Model.UserModel.SetUsers(value); }
        public string avatar = "https://www.google.com.vn/url?sa=i&rct=j&q=&esrc=s&source=images&cd=&cad=rja&uact=8&ved=2ahUKEwiPhMukisLdAhWKbisKHYoNB6kQjRx6BAgBEAU&url=http%3A%2F%2Fwww.technodoze.com%2Fweb-designing%2Fdynamically-change-image-opacity-with.html&psig=AOvVaw3a4xMtNLh7iPeXiNfkaUxO&ust=1537275117586161";
        public string kindSearch = "name";
        ObservableCollection<string> list_search = new ObservableCollection<string>();
        public MainPage()
        {          
            this.InitializeComponent();
            list_search.Add("Email");
            list_search.Add("Name");
            list_search.Add("Phone");           
        }

        private void User_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Entity.User selectedUser = (Entity.User)((StackPanel)sender).Tag;
            this.Name_Detail.Text = selectedUser.Name;
            this.Email_Detail.Text = selectedUser.Email;
            this.Phone_Detail.Text = selectedUser.Phone;
            this.Address_Detail.Text = selectedUser.Address;
            BitmapImage img = new BitmapImage(new Uri(selectedUser.Avatar));
            this.Avatar_Detail.Source = img;
            Pvmain.SelectedItem = User_Detail;
            User_Detail.Visibility = Visibility.Visible; 
        }

      
        private async void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            await this.MyDialog.ShowAsync();
        }
       
        private void OnSubmit(object sender, RoutedEventArgs e)
        {
            Entity.User user = new Entity.User();
            user.Name = this.Name.Text;
            user.Email = this.Email.Text;
            user.Phone = this.Phone.Text;
            user.Address = this.Address.Text;
            user.Avatar = avatar;           
            Model.UserModel.AddUser(user);
            UWPConsole.BackgroundConsole.WriteLine("Add User Success");
            Pvmain.SelectedItem = Home;
        }

        private async void OnSelectImg(object sender, RoutedEventArgs e)
        {
            try
            {
                var uploadUrl = this.GetUploadUrl();
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".png");
                StorageFile file = await openPicker.PickSingleFileAsync();
                if (file != null)
                {
                    // Ẩn nội dung trong nút và hiển thị progress ring
                    this.AvatarBtnContent.Visibility = Visibility.Collapsed;
                    this.AvatarPreview.Visibility = Visibility.Collapsed;
                    this.UploadImgProgress.Visibility = Visibility.Visible;
                    avatar = await this.HttpUploadFile(await uploadUrl, "myFile", "image/png", file);
                    BitmapImage img = new BitmapImage(new Uri(avatar));
                    // Hiển thị ảnh demo ở trong nút, ẩn progress ring.
                    this.AvatarPreview.Source = img;
                    this.AvatarPreview.Visibility = Visibility.Visible;
                    this.UploadImgProgress.Visibility = Visibility.Collapsed;
                    UWPConsole.BackgroundConsole.WriteLine("Upload Img success!");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                UWPConsole.BackgroundConsole.WriteLine("Upload Img failed!");
                Debug.WriteLine(exception.InnerException);
            }
        }

        private async Task<string> GetUploadUrl()
        {
            Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();
            Uri requestUri = new Uri("https://1-dot-backup-server-002.appspot.com/get-upload-token");
            Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();
            httpResponse = await httpClient.GetAsync(requestUri);
            httpResponse.EnsureSuccessStatusCode();
            return await httpResponse.Content.ReadAsStringAsync();
        }

        public async Task<string> HttpUploadFile(string url, string paramName, string contentType, StorageFile file)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            Stream rs = await wr.GetRequestStreamAsync();
            rs.Write(boundarybytes, 0, boundarybytes.Length);
            string header = string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n", paramName, "path_file", contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);
            // write file.
            Stream fileStream = await file.OpenStreamForReadAsync();
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            WebResponse wresp = null;
            wresp = await wr.GetResponseAsync();
            Stream stream2 = wresp.GetResponseStream();
            StreamReader reader2 = new StreamReader(stream2);
            return reader2.ReadToEnd();
        }

        public async void CapturePhoto(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);

            StorageFile photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (photo == null)
            {
                // User cancelled photo capture
                return;
            }

            StorageFolder destinationFolder =
    await ApplicationData.Current.LocalFolder.CreateFolderAsync("PhotoFolder",
        CreationCollisionOption.OpenIfExists);

            await photo.CopyAsync(destinationFolder, "Photo.jpg", NameCollisionOption.ReplaceExisting);
            await photo.DeleteAsync();
          
        }

        private void SubmitSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchInput = SearchInput.Text;
            Users.Clear();
            Users = Model.UserModel.GetUsersSearch(searchInput, kindSearch);
            Debug.WriteLine(Users.Count);
            LabelSearch.Text = "Result Search For " + kindSearch + " : " + searchInput;
            LabelSearch.Visibility = Visibility.Visible;
            MyDialog.Hide();
        }

        private void MyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kindSearch = (string)MyComboBox.SelectedItem;
        }
    }

}

