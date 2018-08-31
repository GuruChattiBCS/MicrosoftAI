using ConsoleApp1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CSHttpClientSample
{
    static class Program
    {
        // Replace <Subscription Key> with your valid subscription key.
        const string subscriptionKey = "c83663514e3b4957a6b12d7bacbbb035";

        // You must use the same region in your REST call as you used to
        // get your subscription keys. For example, if you got your
        // subscription keys from westus, replace "westcentralus" in the URL
        // below with "westus".
        //
        // Free trial subscription keys are generated in the westcentralus region.
        // If you use a free trial subscription key, you shouldn't need to change
        // this region.
        const string uriBase =
            "https://australiaeast.api.cognitive.microsoft.com/vision/v1.0/analyze";
		//const string uriBase = "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Prediction/944ff731-4121-4c51-92ec-41a303ff8356/image?iterationId=144100ac-89bb-4230-8da4-7986cd275e4c";

        static void Main()
        {
			while (true)
			{
            // Get the path and filename to process from the user.
            Console.WriteLine("Analyze an image:");
            Console.Write("Enter the path to the image you wish to analyze: ");
            string imageFilePath = Console.ReadLine();
				imageFilePath = imageFilePath.Replace('"', ' ');

            if (File.Exists(imageFilePath))
            {
                // Make the REST API call.
                Console.WriteLine("\nWait a moment for the results to appear.\n");
                MakeAnalysisRequest(imageFilePath).Wait();
            }
            else
            {
                Console.WriteLine("\nInvalid file path");
            }
				Console.WriteLine("\nPress Enter to continue or N to exit...");
				string input = Console.ReadLine();
				if(input == "n" || input == "N")
				{
					break;
				}
			}
        }

        /// <summary>
        /// Gets the analysis of the specified image file by using
        /// the Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">The image file to analyze.</param>
        static async Task MakeAnalysisRequest(string imageFilePath)
        {
            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                // Request parameters. A third optional parameter is "details".
                string requestParameters =
                    "visualFeatures=Categories,Description,Color,Tags,Faces,ImageType,Adult";

                // Assemble the URI for the REST API Call.
                string uri = uriBase + "?" + requestParameters;

                HttpResponseMessage response;

                // Request body. Posts a locally stored JPEG image.
                byte[] byteData = GetImageAsByteArray(imageFilePath);

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses content type "application/octet-stream".
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // Make the REST API call.
                    response = await client.PostAsync(uri, content);
                }

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();
				var imageDetail = JsonConvert.DeserializeObject<Model>(contentString);
				if (imageDetail != null)
				{
					Console.WriteLine("The Image is of:");
					if (imageDetail.categories != null)
					{
						string category_str = "Category: ";
						foreach (var item in imageDetail.categories)
						{
							category_str += imageDetail.categories[0].name + ", ";
						}
						category_str = category_str.Remove(category_str.Length - 2);
						Console.WriteLine(category_str);
						Console.WriteLine();
					}
					if (imageDetail.faces != null)
					{
						int FacesCount = imageDetail.faces.Length;
						if (FacesCount > 0)
						{
							int count = 0;
							Console.WriteLine("There are " + FacesCount + " in the image");
							foreach (var item in imageDetail.faces)
							{
								count++;
								Console.WriteLine("Deatils of Object" + count);
								Console.WriteLine("Aged: " + item.age);
								Console.WriteLine("Gender: " + item.gender);
							}
							Console.WriteLine();
						}
					}
					if((imageDetail.description != null)&&(imageDetail.description.captions != null))
						Console.WriteLine("Caption: " + imageDetail.description.captions[0].text);
				}
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
        }

        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
    }
}