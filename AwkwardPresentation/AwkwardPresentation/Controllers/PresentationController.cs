﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;
using AwkwardPresentation.Business.Services;
using AwkwardPresentation.Models.Pages;
using AwkwardPresentation.Models.Properties;
using Castle.Core.Internal;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Internal;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Web.Mvc;
using VisitOslo.Infrastructure.Helpers;

namespace AwkwardPresentation.Controllers
{
    [EnableCors(origins: "http://localhost:4200", headers: "*", methods: "*")]
    public class PresentationController : ApiController
    {
        public readonly List<string> WildCardWords = new List<string>
        {
            "meme",
            "comic sans",
            "parody",
            "joke",
            "trump",
            "satire",
            "cat",
            "teenager",
            "monkey",
            //"idiot",
            //"blockchain",
            "drunk",
            //"baby",
            //"sad",
            //"happy",
            //"crying",
            //"art",
            "potato",
            //"iot",
            "weird",
            "how to",
            "guide",
            "inappropiate",
            "evil",
            "awkward"
        };

        private string GetRandomWord()
        {
            var itemsInList = WildCardWords.Count;
            var randomNumber = new Random().Next() % itemsInList;
            bool insertWord = (new Random().Next() % 4) == 0;

            if (insertWord)
            {
                return WildCardWords[randomNumber];
            }
            return null;
        }
            



        [System.Web.Http.HttpGet]
        public int StartSession()
        {
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

            var newPage = contentRepository.GetDefault<PresentationModel>(ContentReference.StartPage);
            newPage.PageName = "New page number " + (new Random().Next());

            // Need AccessLevel.NoAccess to get permission to create and save this new page.
            contentRepository.Save(newPage, SaveAction.Publish, AccessLevel.NoAccess);

            // Just changing the page name to contain its ContentLink ID, to make it more clear in the CMS
            newPage.PageName = "Page nr. " + newPage.ContentLink.ID;
            contentRepository.Save(newPage, SaveAction.Publish, AccessLevel.NoAccess);

            return newPage.ContentLink.ID;
        }


        [System.Web.Http.HttpGet]
        public ActionResult GetSlideData(int id)
        {
            var contentReference = new ContentReference(id);
            if (contentReference == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (id == 0)
            {
                return new JsonDataResult()
                {
                    ContentType = "application/json",
                    Data = new SimpleImageModel()
                    {
                        Url = "https://assets3.thrillist.com/v1/image/2546883/size/tmg-article_tall.jpg",
                        Text = "Some text that caused this image"
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            var contentLoader = ServiceLocator.Current.GetInstance<ContentLoader>();
            var contentLoaderWrapper = ServiceLocator.Current.GetInstance<ContentLoaderWrapper>();
            var images = contentLoader.GetChildren<ImageModel>(contentReference);

            var clickerPage = contentLoaderWrapper.FindPagesOfType<ClickerPage>(ContentReference.StartPage, false).FirstOrDefault();
            var latestSensorData = contentLoaderWrapper.FindPagesOfType<StupidClickerModel>(clickerPage?.ContentLink, false).FirstOrDefault();
            ClickerModel clickerModel = null;

            if (latestSensorData != null)
            {
                clickerModel = new ClickerModel(latestSensorData);
            }


            SimpleImageModel image = null;
            if (images != null && images.Any())
            {
                var firstImage = images.First();

                image = new SimpleImageModel()
                {
                    Url = firstImage.Url,
                    Text = firstImage.Text,
                    ClickerModel = clickerModel
                };
            }


            return new JsonDataResult()
            {
                ContentType = "application/json",
                Data = image,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        [System.Web.Http.HttpPost]
        public async Task<ActionResult> UploadSlideData([FromBody] SimpleImageModel model)
        {
            if (model == null || model.Id == 0)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
            var contentLoaderWrapper = ServiceLocator.Current.GetInstance<ContentLoaderWrapper>();

            var reference = new ContentReference(model.Id);
            var presentation = contentLoaderWrapper.GetPageFromReference<PresentationModel>(reference);


            var images = contentRepository.GetChildren<ImageModel>(reference);
            var prevText = images.FirstOrDefault()?.Text ?? "";


            if (presentation != null)
            {
                var imageList = await ImageProvider.GetImage(model.Text, GetRandomWord(), prevText) as ImageList;
                if (imageList == null)
                {
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);
                }

                var imageModel = contentRepository.GetDefault<ImageModel>(presentation.ContentLink);
                imageModel.PageName = "New imagemodel number" + new Random().Next();
                imageModel.Text = model.Text;
                imageModel.Url = imageList?.Slides?.FirstOrDefault().Url;

                // Need AccessLevel.NoAccess to get permission to create and save this new page.
                contentRepository.Save(imageModel, SaveAction.Publish, AccessLevel.NoAccess);

                // Just changing the page name to contain its ContentLink ID, to make it more clear in the CMS
                imageModel.PageName = "Imagemodel nr. " + imageModel.ContentLink.ID;
                contentRepository.Save(imageModel, SaveAction.Publish, AccessLevel.NoAccess);

                throw new HttpResponseException(HttpStatusCode.Accepted);
            }

            throw new HttpResponseException(HttpStatusCode.BadRequest);
        }

        [System.Web.Http.HttpGet]
        public async Task<ActionResult> SummaryPage(int id)
        {
            var contentReference = new ContentReference(id);
            if (contentReference == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            
            var contentLoader = ServiceLocator.Current.GetInstance<ContentLoader>();
            var presentation = contentLoader.Get<PresentationModel>(contentReference);

            var time = DateTime.Now.Subtract(presentation.Created);

            var totalText = DataSummerizer.TotalText(contentReference);
            var words = totalText.Split(' ');
            var wordsPerMinute = words.Length / time.TotalMinutes;

            var textSummary = await DataSummerizer.TextSummary(totalText);

            var sensorData = DataSummerizer.SensorySummary(presentation.Created);

            return new JsonDataResult()
            {
                ContentType = "application/json",
                Data = new
                {
                    SensoryData = sensorData,
                    TextSummary = textSummary,
                    WordsPerMinute = wordsPerMinute
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

    }
}