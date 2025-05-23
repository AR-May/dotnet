﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows;
using System.Windows.Controls;
using Microsoft.CodeAnalysis;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.InheritanceMargin.MarginGlyph;

internal sealed class MenuItemContainerTemplateSelector : ItemContainerTemplateSelector
{
    // By default, ContextMenu would create same MenuItem for each ViewModel from ItemSource,
    // this would override the default behavior, and let contextMenu create different MenuItem
    // based on the ViewModel's type
    public override DataTemplate SelectTemplate(object item, ItemsControl parentItemsControl)
    {
        if (item is HeaderMenuItemViewModel)
        {
            // Template for Header
            return (DataTemplate)parentItemsControl.FindResource("HeaderMenuItemTemplate");
        }

        if (item is DisambiguousTargetMenuItemViewModel)
        {
            // Template for DisambiguatingTargetMenuItemTemplate, which contains one more icon than the common TargetMenuItemTemplate.
            return (DataTemplate)parentItemsControl.FindResource("DisambiguatingTargetMenuItemTemplate");
        }

        if (item is TargetMenuItemViewModel)
        {
            // Template for a common target
            return (DataTemplate)parentItemsControl.FindResource("TargetMenuItemTemplate");
        }

        if (item is MemberMenuItemViewModel)
        {
            // Template for member
            return (DataTemplate)parentItemsControl.FindResource("MemberMenuItemTemplate");
        }

        throw ExceptionUtilities.Unreachable();
    }
}
