﻿using AwkwardPresentation.Models.Properties;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AwkwardPresentation.Models.Pages
{
    [ContentType(DisplayName = "PresentationModel", GUID = "6bd7cc52-a0fb-4c48-62d9-4c3dfa0a20a4", Description = "")]
    public class ClickerPage : PageData
    {
        [Display(
            Name = "ClickerPage",
            Description = "",
            GroupName = SystemTabNames.Content,
            Order = 110)]
        [AllowedTypes(typeof(ClickerModel))]
        public List<ClickerModel> DataSet { get; set; }
    }
}