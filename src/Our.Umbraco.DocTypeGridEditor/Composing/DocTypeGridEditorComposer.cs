using Our.Umbraco.DocTypeGridEditor.Extensions;
using Our.Umbraco.DocTypeGridEditor.ValueProcessing;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Our.Umbraco.DocTypeGridEditor.Composing
{
    /// <summary>
    /// Composes defaults for Doc Type Grid Editor
    /// </summary>
    public class DocTypeGridEditorComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.DocTypeGridEditorValueProcessors().Append<UmbracoTagsValueProcessor>();
            composition.DataValueReferenceFactories().Append<DocTypeGridEditorDataValueReference>();
            composition.Components().Append<DocTypeGridEditorCacheResetComponent>();
        }
    }

    public class DocTypeGridEditorCacheResetComponent : IComponent
    {
        private IAppPolicyCache _runtimeCache;

        public DocTypeGridEditorCacheResetComponent(AppCaches caches)
        {
            _runtimeCache = caches.RuntimeCache;
        }

        public void Initialize()
        {
            ContentService.Published += ContentServiceOnPublished;
        }

        private void ContentServiceOnPublished(IContentService sender, ContentPublishedEventArgs e)
        {
            // Empty the whole cache, otherwise it seems like referenced values will not be updated.
            // This will remove all entries that start with this string.
            _runtimeCache.ClearByKey($"Our.Umbraco.DocTypeGridEditor.Helpers.DocTypeGridEditorHelper.ConvertValueToContent_Page_");
        }

        public void Terminate()
        {
            
        }
    }

}