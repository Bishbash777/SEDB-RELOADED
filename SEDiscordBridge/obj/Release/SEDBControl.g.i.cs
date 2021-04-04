﻿#pragma checksum "..\..\SEDBControl.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "686550A9987FD5ED94CAC1C8430A527E6B963AFDC266AD35DDD836900CF064CD"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace SEDiscordBridge {
    
    
    /// <summary>
    /// SEDBControl
    /// </summary>
    public partial class SEDBControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 20 "..\..\SEDBControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label popupTarget;
        
        #line default
        #line hidden
        
        
        #line 51 "..\..\SEDBControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cbFontColor;
        
        #line default
        #line hidden
        
        
        #line 87 "..\..\SEDBControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnAddPerm;
        
        #line default
        #line hidden
        
        
        #line 88 "..\..\SEDBControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnDelPerm;
        
        #line default
        #line hidden
        
        
        #line 92 "..\..\SEDBControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtPlayerName;
        
        #line default
        #line hidden
        
        
        #line 96 "..\..\SEDBControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtPermission;
        
        #line default
        #line hidden
        
        
        #line 100 "..\..\SEDBControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid dgPermList;
        
        #line default
        #line hidden
        
        
        #line 196 "..\..\SEDBControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cbFacFontColor;
        
        #line default
        #line hidden
        
        
        #line 202 "..\..\SEDBControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnAddFac;
        
        #line default
        #line hidden
        
        
        #line 203 "..\..\SEDBControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnDelFac;
        
        #line default
        #line hidden
        
        
        #line 206 "..\..\SEDBControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtFacName;
        
        #line default
        #line hidden
        
        
        #line 210 "..\..\SEDBControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtFacChannel;
        
        #line default
        #line hidden
        
        
        #line 214 "..\..\SEDBControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid dgFacList;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/SEDiscordBridge;component/sedbcontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\SEDBControl.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 16 "..\..\SEDBControl.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.SaveConfig_OnClick);
            
            #line default
            #line hidden
            return;
            case 2:
            this.popupTarget = ((System.Windows.Controls.Label)(target));
            return;
            case 3:
            
            #line 27 "..\..\SEDBControl.xaml"
            ((System.Windows.Documents.Hyperlink)(target)).RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler(this.Hyperlink_RequestNavigate);
            
            #line default
            #line hidden
            return;
            case 4:
            this.cbFontColor = ((System.Windows.Controls.ComboBox)(target));
            
            #line 51 "..\..\SEDBControl.xaml"
            this.cbFontColor.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.CbFontColor_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.btnAddPerm = ((System.Windows.Controls.Button)(target));
            
            #line 87 "..\..\SEDBControl.xaml"
            this.btnAddPerm.Click += new System.Windows.RoutedEventHandler(this.btnAddPerm_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.btnDelPerm = ((System.Windows.Controls.Button)(target));
            
            #line 88 "..\..\SEDBControl.xaml"
            this.btnDelPerm.Click += new System.Windows.RoutedEventHandler(this.btnDelPerm_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.txtPlayerName = ((System.Windows.Controls.TextBox)(target));
            return;
            case 8:
            this.txtPermission = ((System.Windows.Controls.TextBox)(target));
            return;
            case 9:
            this.dgPermList = ((System.Windows.Controls.DataGrid)(target));
            
            #line 100 "..\..\SEDBControl.xaml"
            this.dgPermList.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.DgPermList_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 10:
            this.cbFacFontColor = ((System.Windows.Controls.ComboBox)(target));
            
            #line 196 "..\..\SEDBControl.xaml"
            this.cbFacFontColor.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.CbFontColor_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 11:
            this.btnAddFac = ((System.Windows.Controls.Button)(target));
            
            #line 202 "..\..\SEDBControl.xaml"
            this.btnAddFac.Click += new System.Windows.RoutedEventHandler(this.BtnAddFac_Click);
            
            #line default
            #line hidden
            return;
            case 12:
            this.btnDelFac = ((System.Windows.Controls.Button)(target));
            
            #line 203 "..\..\SEDBControl.xaml"
            this.btnDelFac.Click += new System.Windows.RoutedEventHandler(this.BtnDelFac_Click);
            
            #line default
            #line hidden
            return;
            case 13:
            this.txtFacName = ((System.Windows.Controls.TextBox)(target));
            return;
            case 14:
            this.txtFacChannel = ((System.Windows.Controls.TextBox)(target));
            return;
            case 15:
            this.dgFacList = ((System.Windows.Controls.DataGrid)(target));
            
            #line 214 "..\..\SEDBControl.xaml"
            this.dgFacList.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.DgFacList_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 16:
            
            #line 228 "..\..\SEDBControl.xaml"
            ((System.Windows.Documents.Hyperlink)(target)).RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler(this.Hyperlink_RequestNavigate);
            
            #line default
            #line hidden
            return;
            case 17:
            
            #line 240 "..\..\SEDBControl.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.SaveConfig_OnClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
