#pragma checksum "..\..\..\..\Views\THView.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "5FE5CDFA92D1C6F50CCB4A32D459459ED6A8A12A56C394D3472EF1665B2006AD"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using DataSystem.Views;
using ScottPlot;
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


namespace DataSystem.Views {
    
    
    /// <summary>
    /// THView
    /// </summary>
    public partial class THView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 12 "..\..\..\..\Views\THView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal ScottPlot.WpfPlot thPlot;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\..\..\Views\THView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal ScottPlot.WpfPlot ePlot;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\..\Views\THView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal ScottPlot.WpfPlot mPlot;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\..\Views\THView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox comboBox1;
        
        #line default
        #line hidden
        
        
        #line 39 "..\..\..\..\Views\THView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button button2;
        
        #line default
        #line hidden
        
        
        #line 40 "..\..\..\..\Views\THView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RichTextBox richTextBox;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\..\..\Views\THView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button clear_btn;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\..\..\Views\THView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button saveTH;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\..\..\Views\THView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button loadSql;
        
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
            System.Uri resourceLocater = new System.Uri("/DataSystem;component/views/thview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Views\THView.xaml"
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
            this.thPlot = ((ScottPlot.WpfPlot)(target));
            return;
            case 2:
            this.ePlot = ((ScottPlot.WpfPlot)(target));
            return;
            case 3:
            this.mPlot = ((ScottPlot.WpfPlot)(target));
            return;
            case 4:
            this.comboBox1 = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 5:
            this.button2 = ((System.Windows.Controls.Button)(target));
            
            #line 39 "..\..\..\..\Views\THView.xaml"
            this.button2.Click += new System.Windows.RoutedEventHandler(this.button2_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.richTextBox = ((System.Windows.Controls.RichTextBox)(target));
            return;
            case 7:
            this.clear_btn = ((System.Windows.Controls.Button)(target));
            
            #line 47 "..\..\..\..\Views\THView.xaml"
            this.clear_btn.Click += new System.Windows.RoutedEventHandler(this.clear_btn_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.saveTH = ((System.Windows.Controls.Button)(target));
            
            #line 48 "..\..\..\..\Views\THView.xaml"
            this.saveTH.Click += new System.Windows.RoutedEventHandler(this.saveTH_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            this.loadSql = ((System.Windows.Controls.Button)(target));
            
            #line 49 "..\..\..\..\Views\THView.xaml"
            this.loadSql.Click += new System.Windows.RoutedEventHandler(this.loadSql_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

