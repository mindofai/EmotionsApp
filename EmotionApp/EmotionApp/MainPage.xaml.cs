using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
namespace EmotionApp
{
    public partial class MainPage : ContentPage
    {
        private Stream stream;

        public MainPage()
        {
            InitializeComponent();

            pickPhoto.Clicked += async (sender, args) =>
            {
                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
                    return;
                }
                var file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                {
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,

                });


                if (file == null)
                    return;

                activityIndicator.IsVisible = true;
                stream = file.GetStream();
                await IdentifyEmotion(stream);

                activityIndicator.IsVisible = false;

                image.Source = ImageSource.FromStream(() =>
                {
                    stream = file.GetStream();
                    file.Dispose();
                    return stream;
                });
            };
        }

        private async void TakePhoto(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "Test",
                SaveToAlbum = true,
                CompressionQuality = 75,
                CustomPhotoSize = 50,
                PhotoSize = PhotoSize.MaxWidthHeight,
                MaxWidthHeight = 2000,
                DefaultCamera = CameraDevice.Front
            });

            if (file == null)
                return;

            activityIndicator.IsVisible = true;
            stream = file.GetStream();
            await IdentifyEmotion(stream);
            activityIndicator.IsVisible = false;
            image.Source = ImageSource.FromStream(() =>
            {
                stream = file.GetStream();
                file.Dispose();
                return stream;
            });
        }

        private async Task IdentifyEmotion(Stream stream)
        {
            FaceClient faceClient = new FaceClient(
       new ApiKeyServiceClientCredentials("0ccd2300a2b54e8caed6562aaedaca1e"),
       new System.Net.Http.DelegatingHandler[] { });
            faceClient.Endpoint = "https://southeastasia.api.cognitive.microsoft.com/";

            IList<DetectedFace> faceList =
                    await faceClient.Face.DetectWithStreamAsync(
                        stream, true, false, new FaceAttributeType[] { FaceAttributeType.Emotion, FaceAttributeType.Age });
            if (faceList != null && faceList.Count > 0)
            {
                string result = "";
                if (faceList[0].FaceAttributes.Emotion.Happiness > 0.70)
                    result = "HAPPY :)";
                else if (faceList[0].FaceAttributes.Emotion.Sadness > 0.70)
                    result = "SAD :(";
                else if (faceList[0].FaceAttributes.Emotion.Fear > 0.70)
                    result = "SCARED :s";
                else if (faceList[0].FaceAttributes.Emotion.Anger > 0.70)
                    result = "ANGRY >:(";
                else if (faceList[0].FaceAttributes.Emotion.Surprise > 0.70)
                    result = "SURPRISED :O";
                else if (faceList[0].FaceAttributes.Emotion.Disgust > 0.70)
                    result = "DISGUSTED xO";
                else if (faceList[0].FaceAttributes.Emotion.Contempt > 0.70)
                    result = "CONTEMPT :/";
                else
                    result = "EMOTIONLESS";

                resultText.Text = $"You look like you're\n {result}";
            }
            else
                resultText.Text = $"No face found";

        }

    }
}
