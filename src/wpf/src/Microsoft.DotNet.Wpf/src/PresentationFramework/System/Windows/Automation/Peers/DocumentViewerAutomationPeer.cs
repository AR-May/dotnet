﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers
{
    /// <summary>
    /// AutomationPeer associated with DocumentViewer
    /// </summary>
    public class DocumentViewerAutomationPeer : DocumentViewerBaseAutomationPeer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="owner">Owner of the AutomationPeer.</param>
        public DocumentViewerAutomationPeer(DocumentViewer owner)
            : base(owner)
        { }

        /// <summary>
        /// <see cref="AutomationPeer.GetClassNameCore"/>
        /// </summary>
        protected override string GetClassNameCore()
        {
            return "DocumentViewer";
        }

        /// <summary>
        /// <see cref="AutomationPeer.GetPattern"/>
        /// </summary>
        public override object GetPattern(PatternInterface patternInterface)
        {
            object returnValue = null;

            // Check if provided patternInterface is for Scroll, which is all
            // that is currently exposed.
            if (patternInterface == PatternInterface.Scroll)
            {
                // Get a reference to DocumentViewer's ScrollViewer
                DocumentViewer owner = (DocumentViewer)Owner;
                if (owner.ScrollViewer != null)
                {
                    // Get a reference to ScrollViewer's AutomationPeer.
                    AutomationPeer scrollPeer = UIElementAutomationPeer.CreatePeerForElement(owner.ScrollViewer);
                    if (scrollPeer != null && scrollPeer is IScrollProvider)
                    {
                        scrollPeer.EventsSource = this;
                        returnValue = scrollPeer;
                    }
                }
            }
            else
            {
                returnValue = base.GetPattern(patternInterface);
            }

            return returnValue;
        }
    }
}

