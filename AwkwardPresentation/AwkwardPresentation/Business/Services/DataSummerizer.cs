﻿using AwkwardPresentation.Models.Pages;
using AwkwardPresentation.Models.Properties;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AwkwardPresentation.Business.Services
{
    public class DataSummerizer
    {
        static IContentRepository contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

        public static IEnumerable<ClickerModel> SensorySummary(DateTime startTime)
        {
            var clickerPage = contentRepository.GetChildren<ClickerPage>(ContentReference.StartPage).FirstOrDefault();

            if (clickerPage == null)
                return null;

            var repoData = contentRepository.GetChildren<StupidClickerModel>(clickerPage.ContentLink);
            var dateData = repoData?.Where(d => d.Created > startTime);
            var data = dateData?.Select(d => new ClickerModel(d));

            return data;
        }

        public async static Task<string> TextSummary(string text)
        {
            if (text == null || text == "")
                text = "Religious texts (also known as scripture, or scriptures, from the Latin scriptura, meaning writing) are texts which religious traditions consider to be central to their practice or beliefs. Religious texts may be used to provide meaning and purpose, evoke a deeper connection with the divine, convey religious truths, promote religious experience, foster communal identity, and guide individual and communal religious practice. Religious texts often communicate the practices or values of a religious traditions and can be looked to as a set of guiding principles which dictate physical, mental, spiritual, or historical elements considered important to a specific religion. The terms 'sacred' text and 'religious' text are not necessarily interchangeable in that some religious texts are believed to be sacred because of their nature as divinely or supernaturally revealed or inspired, whereas some religious texts are simply narratives pertaining to the general themes, practices, or important figures of the specific religion, and not necessarily considered sacred by itself. A core function of a religious text making it sacred is its ceremonial and liturgical role, particularly in relation to sacred time, the liturgical year, the divine efficacy and subsequent holy service; in a more general sense, its performance.";
            
            var textObject = JsonConvert.SerializeObject(new
            {
                text = text
            });

            var url = "http://text2slides.westeurope.cloudapp.azure.com/summary";

            var summary = await ImageProvider.SendJsonRequest(textObject, url);

            return summary?.ToString() ?? "";
        }

        public static string TotalText(ContentReference reference)
        {
            var imageModels = contentRepository.GetChildren<ImageModel>(reference);
            var texts = imageModels.Select(i => i.Text + " ");
            return string.Concat(texts);
        }
    }
}