using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.LanguageServer.VsCodeProtocol;

namespace TypeCobol.LanguageServer.TypeCobolCustomLanguageServerProtocol
{
    class RefreshOutlineParams
    {
        /// <summary>
        /// The Text Document Identifier
        /// </summary>
        public TextDocumentItem textDocument;


        /// <summary>
        /// The List of OutlineNode concerned
        /// </summary>
        public OutlineNode outlineNode;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="document">The Document's identifier</param>
        /// <param name="outlineNodes">The List of concerned OutlineNode</param>
        public RefreshOutlineParams(TextDocumentItem document, OutlineNode outlineNodes)
        {
            this.textDocument = document;
            this.outlineNode = outlineNodes;
        }
    }
}
