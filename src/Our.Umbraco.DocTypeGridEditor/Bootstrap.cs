﻿using System;
using System.Web.Mvc;
using Newtonsoft.Json;
using Our.Umbraco.DocTypeGridEditor.Web.Attributes;
using Our.Umbraco.DocTypeGridEditor.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;
using Umbraco.Web.Routing;

namespace Our.Umbraco.DocTypeGridEditor
{
    internal class Bootstrap : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            GlobalFilters.Filters.Add(new DocTypeGridEditorPreviewAttribute());

            if (DefaultDocTypeGridEditorSurfaceControllerResolver.HasCurrent == false)
            {
                DefaultDocTypeGridEditorSurfaceControllerResolver.Current = new DefaultDocTypeGridEditorSurfaceControllerResolver();
            }
        }

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            DataTypeCacheRefresher.CacheUpdated += (sender, e) =>
            {
                if (e.MessageType == MessageType.RefreshByJson)
                {
                    var payload = JsonConvert.DeserializeAnonymousType((string)e.MessageObject, new[] { new { Id = default(int), UniqueId = default(Guid) } });
                    if (payload != null)
                    {
                        foreach (var item in payload)
                        {
                            applicationContext.ApplicationCache.RuntimeCache.ClearCacheItem(
                                string.Concat("Our.Umbraco.DocTypeGridEditor.Web.Extensions.ContentTypeServiceExtensions.GetAliasById_", item.UniqueId));

                            applicationContext.ApplicationCache.RuntimeCache.ClearCacheItem(
                                string.Concat("Our.Umbraco.DocTypeGridEditor.Helpers.DocTypeGridEditorHelper.GetPreValuesCollectionByDataTypeId_", item.Id));
                        }
                    }
                }
            };

            ContentTypeCacheRefresher.CacheUpdated += (sender, e) =>
            {
                if (e.MessageType == MessageType.RefreshByJson)
                {
                    var payload = JsonConvert.DeserializeAnonymousType((string)e.MessageObject, new[] { new { Alias = default(string) } });
                    if (payload != null)
                    {
                        foreach (var item in payload)
                        {
                            applicationContext.ApplicationCache.RuntimeCache.ClearCacheItem(
                                string.Concat("Our.Umbraco.DocTypeGridEditor.Helpers.DocTypeGridEditorHelper.GetContentTypesByAlias_", item.Alias));

                            // NOTE: Unsure how to get the doctype GUID, without hitting the database?
                            // So we end up clearing the entire cache for this key. [LK:2018-01-30]
                            applicationContext.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(
                                "Our.Umbraco.DocTypeGridEditor.Helpers.DocTypeGridEditorHelper.GetContentTypeAliasByGuid_");
                        }
                    }
                }
            };

            PublishedContentRequest.Prepared += PublishedContentRequest_Prepared;
        }

        private void PublishedContentRequest_Prepared(object sender, EventArgs e)
        {
            var request = sender as PublishedContentRequest;
            // Check if it's a dtgePreview request and is set to redirect.
            // If so reset the redirect url to an empty string to stop the redirect happening in preview mode.
            if (request.Uri.Query.Contains("dtgePreview") && request.IsRedirect)
            {
                request.SetRedirect(string.Empty);
            }
        }
    }
}