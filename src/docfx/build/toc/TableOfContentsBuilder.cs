// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Linq;

namespace Microsoft.Docs.Build
{
    internal class TableOfContentsBuilder
    {
        private readonly Config _config;
        private readonly TableOfContentsLoader _tableOfContentsLoader;
        private readonly ContentValidator _contentValidator;
        private readonly MetadataProvider _metadataProvider;
        private readonly MetadataValidator _metadataValidator;
        private readonly DocumentProvider _documentProvider;
        private readonly MonikerProvider _monikerProvider;
        private readonly PublishModelBuilder _publishModelBuilder;
        private readonly TemplateEngine _templateEngine;
        private readonly Output _output;

        public TableOfContentsBuilder(
            Config config,
            TableOfContentsLoader tableOfContentsLoader,
            ContentValidator contentValidator,
            MetadataProvider metadataProvider,
            MetadataValidator metadataValidator,
            DocumentProvider documentProvider,
            MonikerProvider monikerProvider,
            PublishModelBuilder publishModelBuilder,
            TemplateEngine templateEngine,
            Output output)
        {
            _config = config;
            _tableOfContentsLoader = tableOfContentsLoader;
            _contentValidator = contentValidator;
            _metadataProvider = metadataProvider;
            _metadataValidator = metadataValidator;
            _documentProvider = documentProvider;
            _monikerProvider = monikerProvider;
            _publishModelBuilder = publishModelBuilder;
            _templateEngine = templateEngine;
            _output = output;
        }

        public void Build(ErrorBuilder errors, FilePath file)
        {
            // load toc tree
            var (node, _, _, _) = _tableOfContentsLoader.Load(file);

            _contentValidator.ValidateTocDeprecated(file);

            var metadata = _metadataProvider.GetMetadata(errors, file);
            _metadataValidator.ValidateMetadata(errors, metadata.RawJObject, file);

            var tocMetadata = JsonUtility.ToObject<TableOfContentsMetadata>(errors, metadata.RawJObject);

            var path = _documentProvider.GetSitePath(file);

            var model = new TableOfContentsModel(node.Items.Select(item => item.Value).ToArray(), tocMetadata, path);

            var outputPath = _documentProvider.GetOutputPath(file);

            // enable pdf
            if (_config.OutputPdf)
            {
                var monikers = _monikerProvider.GetFileLevelMonikers(errors, file);
                model.Metadata.PdfAbsolutePath = "/" +
                    UrlUtility.Combine(
                        _config.BasePath, "opbuildpdf", monikers.MonikerGroup ?? "", LegacyUtility.ChangeExtension(path, ".pdf"));
            }

            if (!errors.FileHasError(file) && !_config.DryRun)
            {
                if (_config.OutputType == OutputType.Html)
                {
                    if (_documentProvider.GetRenderType(file) == RenderType.Content)
                    {
                        var viewModel = _templateEngine.RunJavaScript($"toc.html.js", JsonUtility.ToJObject(model));
                        var html = _templateEngine.RunMustache($"toc.html", viewModel, file);
                        _output.WriteText(outputPath, html);
                    }

                    // Just for current PDF build. toc.json is used for generate PDF outline
                    var output = _templateEngine.RunJavaScript("toc.json.js", JsonUtility.ToJObject(model));
                    _output.WriteJson(Path.ChangeExtension(outputPath, ".json"), output);
                }
                else
                {
                    var output = _templateEngine.RunJavaScript("toc.json.js", JsonUtility.ToJObject(model));
                    _output.WriteJson(outputPath, output);
                }
            }

            _publishModelBuilder.SetPublishItem(file, metadata: null, outputPath);
        }
    }
}
