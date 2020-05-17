using clipboardplus.Model;
using HTMLConverter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;
using System.Xml;
using Path = System.IO.Path;

namespace clipboardplus.Controls
{
    /// <summary>
    /// YanaRichTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class YanaRichTextBox : UserControl
    {
        public YanaRichTextBox()
        {
            InitializeComponent();

            //字体大小
            Enumerable.Range(1, 100).ToList().ForEach(i => fontsizes.Add(new fontsize() { Id = i }));
            cb_FontSIze.ItemsSource = fontsizes;

            //RichTextBox
            DataObject.AddPastingHandler(richTextBox, new DataObjectPastingEventHandler(OnPaste));
            // Document.PageWidth = 1024;
            PreviewMouseDown += richTextBox_PreviewMouseDown;
            PreviewMouseUp += richTextBox_PreviewMouseUp;
            richTextBox.PreviewKeyDown += richTextBox_PreviewKeyDown;


            this.SizeChanged += richTextBox_SizeChanged;

            Style style = new Style(typeof(Paragraph));
            style.Setters.Add(new Setter(Block.MarginProperty, new Thickness(0)));
            richTextBox.Document.Resources.Add(typeof(Paragraph), style);


            richTextBox.Document.PagePadding = PAGE_PADDING;
            richTextBox.Document.ColumnWidth = richTextBox.Document.PageWidth;

            search_helper = new SearchHelper(richTextBox);

            richTextBox.SelectionChanged += richTextBox_SelectionChanged;


            richTextBox.PreviewKeyDown += richTextBox_PreviewKeyDown;
            UpdateVisualState();
        }        
        List<fontsize> fontsizes = new List<fontsize>();

        public RichTextBox Editer
        {
            get { return this.richTextBox; }
        }

        public Button SaveBtn
        {
            get { return this.saveBtn; }
        }

        private bool updateRTB = false;

        public bool isCtrl()
        {
            return (Keyboard.Modifiers == ModifierKeys.Control);
        }

        public bool isCtrlShift()
        {
            return (Keyboard.Modifiers ==
                (ModifierKeys.Control | ModifierKeys.Shift));
        }

        private void richTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (isCtrl())
            {
                switch (e.Key)
                {
                    case Key.Q:
                        SearchTermTextBox.Focus();
                        break;
                }
            }
            switch (e.Key)
            {
                case Key.Enter:
                    //DetectURL();
                    //if (Keyboard.Modifiers == ModifierKeys.Control)
                    //{
                    //    this.cmdInsertParagraphBreak();
                    //}
                    //else
                    //{
                    //    this.cmdInsertLine();
                    //}
                    //e.Handled = true;

                    break;
                case Key.Space:
                case Key.Tab:
                    DetectURL();
                    break;
                case Key.B:

                    break;
            }
        }

        private void richTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateVisualState();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set { SetValue(HtmlProperty, value); }
        }

        bool updateUI = true;

        public static DependencyProperty HtmlProperty =
        DependencyProperty.Register("Html", typeof(string), typeof(YanaRichTextBox), new PropertyMetadata("", new PropertyChangedCallback(OnHtmlChange)));

        private static void OnHtmlChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d as YanaRichTextBox).updateUI)
            {
                Console.WriteLine("\n\n\n\n\n\nHtml\n" + d.GetType().ToString() + "\n" + e.OldValue + "\n" + e.NewValue + "\n更新控件\n\n\n\n\n\n");
                if (e.NewValue.ToString().ToUpper().StartsWith("<HTML>"))
                {
                    var yrtb = d as YanaRichTextBox;
                    TextRange textRange = new TextRange(yrtb.Editer.Document.ContentStart, yrtb.Editer.Document.ContentEnd);
                    string originText = textRange.Text;
                    yrtb.cmdSelectAll();
                    yrtb.cmdDelete();
                    Section s = HTMLConverter.HTMLToFlowConverter.ConvertHtmlToSection(e.NewValue.ToString(), yrtb.Editer.Document.PageWidth);//
                    yrtb.cmdInsertBlock(s, true);
                    HyperlinkHelper.SubscribeToAllHyperlinks(yrtb.Editer.Document);
                    if (textRange.Text == "")
                    {
                        textRange.Text = originText;
                    }
                }
            }
            else
            {
                Console.WriteLine("\n\n\n\n\n\nHtml\n" + d.GetType().ToString() + "\n" + e.OldValue + "\n" + e.NewValue + "\n更新源\n\n\n\n\n\n");
                //(d as YanaRichTextBox).GetBindingExpression(TextProperty).UpdateSource();                
            }
            (d as YanaRichTextBox).updateRTB = false;
        }

        public static DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(YanaRichTextBox), new PropertyMetadata("",new PropertyChangedCallback(OnTextChange)));

        private static void OnTextChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d as YanaRichTextBox).updateUI)
            {
                Console.WriteLine("\n\n\n\n\n\nText\n" + d.GetType().ToString() + "\n" + e.OldValue + "\n" + e.NewValue + "\n更新控件\n\n\n\n\n\n");
                var yrtb = d as YanaRichTextBox;
                yrtb.cmdSelectAll();
                yrtb.cmdDelete();
                yrtb.cmdInsertText(e.NewValue as string);
            }
            else
            {
                Console.WriteLine("\n\n\n\n\n\nText\n" + d.GetType().ToString() + "\n" + e.OldValue + "\n" + e.NewValue + "\n更新源\n\n\n\n\n\n");
                //(d as YanaRichTextBox).GetBindingExpression(TextProperty).UpdateSource();
            }
            (d as YanaRichTextBox).updateRTB = false;
        }

        #region Update View

        bool isUpdateVisualStyle = false;

        private void UpdateVisualState()
        {
            isUpdateVisualStyle = true;
            cb_FontName.SelectedItem = SelectionFontFamily;
            cb_FontSIze.SelectedItem = SelectionFontSizePoints;

            UpdateTableState();

            isUpdateVisualStyle = false;

        }

        private void UpdateTableState()
        {
            //if (richTextBox.isTable)
            //{
            //    sp_Table.Visibility = Visibility.Visible;
            //    Thickness thick = richTextBox.TableBorderTickness;

            //    bt_Left.isChecked = (thick.Left > 0 ? true : false);
            //    bt_Right.isChecked = (thick.Right > 0 ? true : false);
            //    bt_Top.isChecked = (thick.Top > 0 ? true : false);
            //    bt_Bottom.isChecked = (thick.Bottom > 0 ? true : false);
            //}
            //else
            //{
            //    sp_Table.Visibility = Visibility.Collapsed;
            //}
        }

        #endregion

        private void UndoClick(object sender, RoutedEventArgs e)
        {
            richTextBox.Focus();
            cmdUndo();
        }

        private void RedoClick(object sender, RoutedEventArgs e)
        {
            richTextBox.Focus();
            cmdRedo();
        }

        private void AlignLeftClick(object sender, RoutedEventArgs e)
        {
            richTextBox.Focus();
            cmdExecute(EditingCommands.AlignLeft);
        }

        private void AlignCenterClick(object sender, RoutedEventArgs e)
        {
            richTextBox.Focus();
            cmdExecute(EditingCommands.AlignCenter);
        }

        private void AlignRightClick(object sender, RoutedEventArgs e)
        {
            richTextBox.Focus();
            cmdExecute(EditingCommands.AlignRight);
        }

        private void AlignJustifyClick(object sender, RoutedEventArgs e)
        {
            richTextBox.Focus();
            cmdExecute(EditingCommands.AlignJustify);

        }

        private void ToggleNumberingClick(object sender, RoutedEventArgs e)
        {
            richTextBox.Focus();
            cmdExecute(EditingCommands.ToggleNumbering);
        }

        private void ToggleBulletsClick(object sender, RoutedEventArgs e)
        {
            richTextBox.Focus();
            cmdExecute(EditingCommands.ToggleBullets);
        }

        private void DecreaseIndentationClick(object sender, RoutedEventArgs e)
        {
            richTextBox.Focus();
            cmdExecute(EditingCommands.DecreaseIndentation);
        }

        private void IncreaseIndentationClick(object sender, RoutedEventArgs e)
        {
            richTextBox.Focus();
            cmdExecute(EditingCommands.IncreaseIndentation);
        }

        private void ToggleBoldClick(object sender, RoutedEventArgs e)
        {
            richTextBox.Focus();
            cmdBold();
        }

        private void ToggleItalicClick(object sender, RoutedEventArgs e)
        {
            richTextBox.Focus();
            cmdItalic();
        }

        private void ToggleUnderlineClick(object sender, RoutedEventArgs e)
        {
            richTextBox.Focus();
            cmdUnderline();
        }

        private void DecreaseFontSizeClick(object sender, RoutedEventArgs e)
        {
            richTextBox.Focus();
            cmdDecreaseFontSize();
        }

        private void IncreaseFontSizeClick(object sender, RoutedEventArgs e)
        {
            richTextBox.Focus();
            cmdIncreaseFontSize();
        }

        private void fontColorSelected(object sender, HandyControl.Data.FunctionEventArgs<Color> e)
        {
            richTextBox.Focus();
            SelectionForeground = new SolidColorBrush(e.Info);
            fontColor.IsChecked = false;
            e.Handled = true;
        }

        private void fontColorDeselected(object sender, EventArgs e)
        {
            richTextBox.Focus();
            SelectionForeground = new SolidColorBrush(Color.FromRgb(21, 21, 21));
            fontColor.IsChecked = false;
        }

        private void fontBackgroundSelected(object sender, HandyControl.Data.FunctionEventArgs<Color> e)
        {
            richTextBox.Focus();
            SelectionBackground = new SolidColorBrush(e.Info);
            fontBackground.IsChecked = false;
            e.Handled = true;
        }

        private void fontBackgroundDeselected(object sender, EventArgs e)
        {
            richTextBox.Focus();
            SelectionBackground = null;
            fontBackground.IsChecked = false;
        }

        private void selectedToText(object sender, RoutedEventArgs e)
        {
            //获取选中部分的开始位置 
            TextPointer textPointer = richTextBox.Selection.Start;
            //Hyperlink
            Run run = new Run(richTextBox.Selection.Text, textPointer);
            //在插入内容的结尾到原来选中部分的结尾——原来选中部分的文字 清除掉 
            TextPointer pointer = run.ContentEnd;
            TextRange textRange = new TextRange(pointer, richTextBox.Selection.End);
            textRange.Text = "";
            //为超链接绑定事件
            textHyperlink.IsChecked = false;
            e.Handled = true;
        }

        private void selectedToLink(object sender, RoutedEventArgs e)
        {
            try
            {
                //获取选中部分的开始位置 
                TextPointer textPointer = richTextBox.Selection.Start;
                //Hyperlink
                Hyperlink hyperlink = new Hyperlink(textPointer, textPointer);
                hyperlink.Inlines.Add(richTextBox.Selection.Text);
                hyperlink.NavigateUri = new Uri(linkText.Text);
                //在插入内容的结尾到原来选中部分的结尾——原来选中部分的文字 清除掉 
                TextPointer pointer = hyperlink.ContentEnd;
                TextRange textRange = new TextRange(pointer, richTextBox.Selection.End);
                textRange.Text = "";
                //为超链接绑定事件
                HyperlinkHelper.SubscribeToAllHyperlinks(Editer.Document);
                textHyperlink.IsChecked = false;
            }
            catch (Exception error)
            {
                MessageBox.Show("路径有误，请重新输入！");
                Console.WriteLine(error.Message);
            }
            e.Handled = true;
        }

        private void fontNameSelected(object sender, SelectionChangedEventArgs e)
        {
            if (isUpdateVisualStyle) return;
            SelectionFontFamily = (FontFamily)cb_FontName.SelectedItem;
            richTextBox.Focus();
        }

        private void fontSizeSelected(object sender, SelectionChangedEventArgs e)
        {
            if (isUpdateVisualStyle) return;
            Console.WriteLine(cb_FontSIze.SelectedItem.GetType());
            SelectionFontSizePoints = (cb_FontSIze.SelectedItem as fontsize).Name;
            richTextBox.Focus();
        }

        private void toolBarShow(object sender, RoutedEventArgs e)
        {
            Grid.SetRowSpan(rtbGrid, 1);
            Grid.SetRow(rtbGrid, 2);
        }

        private void toolBarHide(object sender, RoutedEventArgs e)
        {
            Grid.SetRowSpan(rtbGrid, 2);
            Grid.SetRow(rtbGrid, 1);
        }

        #region 搜索框

        private void SearchTermTextBox_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                if (SearchTermTextBox.Text.Length > 0)
                {
                    Search(SearchTermTextBox.Text);
                }
            }
        }

        private void SearchTermTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTermTextBox.Text.Length == 0)
            {
                SearchClear();
            }
        }
        #endregion

        #region RichTextBox

        public void ClearAdorners()
        {
            search_helper.ClearSearchHelper();
            image_helper.ClearImageResizers(richTextBox);
        }


        #region Search Highligh

        private void UpdateAdorners()
        {
            search_helper.UpdateSearchHelper();
        }


        public void SearchClear()
        {
            search_helper.ClearSearchHelper();
        }

        public void Search(string text)
        {
            search_helper.InitSearchHelper();
            string t = this.PlainText;
            TextRange tr = YanaHelper.FindText(Document.ContentStart, Document.ContentEnd, text, YanaHelper.FindFlags.None, System.Globalization.CultureInfo.CurrentCulture);


            if (tr != null)
                cmdScroll(tr.Start);
            while (tr != null)
            {

                search_helper.AddSearchHelper(tr);
                tr = YanaHelper.FindText(tr.End, Document.ContentEnd, text, YanaHelper.FindFlags.None, System.Globalization.CultureInfo.CurrentCulture);
            }

            //for (TextPointer position = this.Document.ContentStart;
            //position != null && position.CompareTo(Document.ContentEnd) <= 0;
            //position = position.GetNextContextPosition(LogicalDirection.Forward))
            //    {
            //        if (position.CompareTo(Document.ContentEnd) == 0)
            //        {
            //            return;
            //        }
            //        String textRun = position.GetTextInRun(LogicalDirection.Forward);
            //        StringComparison stringComparison = StringComparison.CurrentCulture;
            //        Int32 indexInRun = textRun.IndexOf(text, stringComparison);
            //        if (indexInRun >= 0)
            //        {
            //            position = position.GetPositionAtOffset(indexInRun);
            //            if (position != null)
            //            {
            //                TextPointer nextPointer = position.GetPositionAtOffset(text.Length);
            //                TextRange textRange = new TextRange(position, nextPointer);
            //                textRange.ApplyPropertyValue(TextElement.BackgroundProperty,
            //                              new SolidColorBrush(Colors.Yellow));
            //            }
            //        }
            //    }
        }


        #endregion

        public static readonly Thickness PAGE_PADDING = new Thickness(5);

        private ImageHelper image_helper = new ImageHelper();
        private SearchHelper search_helper;



        FixedDocument _current = null;
        public FixedDocument DocumentFixedGet()
        {
            FlowDocument fd = richTextBox.Document;
            double pw = richTextBox.Document.PageWidth;

            fd.PageWidth = PrintLayout.A4Narrow.Size.Width;
            fd.ColumnWidth = PrintLayout.A4Narrow.ColumnWidth;
            fd.PageHeight = PrintLayout.A4Narrow.Size.Height;
            fd.PagePadding = PrintLayout.A4Narrow.Margin;


            var paginator = ((IDocumentPaginatorSource)richTextBox.Document).DocumentPaginator;

            var package = System.IO.Packaging.Package.Open(new MemoryStream(), FileMode.Create, FileAccess.ReadWrite);
            var packUri = new Uri("pack://temporary.xps");
            System.IO.Packaging.PackageStore.RemovePackage(packUri);
            System.IO.Packaging.PackageStore.AddPackage(packUri, package);
            var xps = new System.Windows.Xps.Packaging.XpsDocument(package, System.IO.Packaging.CompressionOption.NotCompressed, packUri.ToString());
            System.Windows.Xps.Packaging.XpsDocument.CreateXpsDocumentWriter(xps).Write(paginator);
            _current = xps.GetFixedDocumentSequence().References[0].GetDocument(true);

            fd.PageWidth = pw;
            fd.PagePadding = PAGE_PADDING;

            return _current;

        }


        public static T TryFindParent<T>(DependencyObject child)
            where T : DependencyObject
        {
            return YanaHelper.TryFindParent<T>(child);
        }

        public static DependencyObject GetParentObject(DependencyObject child)
        {
            return YanaHelper.GetParentObject(child);
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            return YanaHelper.FindVisualChildren<T>(depObj);
        }

        public Span GetImage(
            BitmapSource image,
            ImageContentType contnt_type = ImageContentType.ImagePngContentType,
            Stretch stretch = Stretch.Fill)
        {
            return YanaHelper.GetImage(image, contnt_type, stretch);
        }

        //./images enforce*/
        public FlowDocument Document
        {
            get { return richTextBox.Document; }

            set { this.richTextBox.Document = value; }
        }


        private void richTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double nw = e.NewSize.Width - 10;
            this.PageWidth = (nw < this.MinWidth ? MinWidth : nw);
            UpdateAdorners();
        }

        private void DetectURL()
        {
            TextPointer position = this.CaretPosition;
            string textRun = position.GetTextInRun(LogicalDirection.Backward);
            int pos = 0;
            for (int i = textRun.Length - 1; i >= 0; i--)
            {
                char c = textRun[i];
                if (char.IsWhiteSpace(c))
                {
                    pos = i + 1; break;
                }
            }

            int offs = textRun.Length - pos;
            string original; string urlp = original = textRun.Substring(pos, offs).Trim();
            Uri uriResult;
            if (YanaHelper.IsHyperlink(ref urlp, out uriResult))
            {
                //Replace URL with hyperlink

                TextPointer tb = position.GetPositionAtOffset(-offs);
                tb.DeleteTextInRun(offs);
                // richTextBox.Selection.Select(tb, position);

                //TextRange tr = new TextRange(position, tb);
                //this.CaretPosition.Ins
                Hyperlink h = new Hyperlink(tb, tb);
                h.Cursor = Cursors.Hand;
                h.NavigateUri = uriResult;
                h.Inlines.Add(new Run(original));
                h.FontSize = 16;
                this.CaretPosition = h.ElementEnd;
                //richTextBox.Selection.Select(tb, position);
                //cmdDelete();
                //cmdInsertInline(h);
            }
        }

        public TextPointer CaretPosition
        {
            get { return richTextBox.CaretPosition; }
            set { this.richTextBox.CaretPosition = value; }
        }

        public int CurrentPoistionValue
        {
            get
            {
                return richTextBox.Document.ContentStart.GetOffsetToPosition(richTextBox.CaretPosition);
            }
            set
            {
                richTextBox.CaretPosition = richTextBox.Document.ContentStart.GetPositionAtOffset(value, LogicalDirection.Forward);
            }
        }


        public void LoadFile(string filename)
        {
            try
            {
                string ext = Path.GetExtension(filename).ToUpper();
                switch (ext)
                {
                    case ".DOCX":
                        LoadFileDocX(filename);
                        break;
                    case ".XPS":
                        LoadFileXPS(filename);
                        break;
                    case ".XAML":
                    default:
                        LoadFileXAML(filename);
                        break;
                }
            }
            catch (Exception)
            {

            }

        }

        public void LoadFileDocX(string filename)
        {
            using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {

                var flowDocumentConverter = new DocxReaderApplication.DocxToFlowDocumentConverter(stream);
                flowDocumentConverter.Read();
                this.Document = flowDocumentConverter.Document;
            }
        }

        public void LoadFileXPS(string filename)
        {
            FlowDocument doc = DocEdLib.XPS.XPStoFlowDocument.Convert(filename);
            if (doc != null)
            {
                richTextBox.Document = doc;
            }
        }

        public void LoadFileXAML(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            this.Load(fs);
        }


        public void LoadHTML(string html)
        {

            Load();
            Section s = HTMLConverter.HTMLToFlowConverter.ConvertHtmlToSection(html, richTextBox.Document.PageWidth);
            this.cmdInsertBlock(s);
            HyperlinkHelper.SubscribeToAllHyperlinks(richTextBox.Document);
        }

        /**/
        public void Load()
        {
            richTextBox.Document.Blocks.Clear();
        }

        public void Load(Stream stream)
        {
            var content = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            content.Load(stream, System.Windows.DataFormats.XamlPackage);
        }

        public void Save(Stream stream)
        {
            byte[] b = this.ComproessedDocument;
            stream.Write(b, 0, b.Length);

            stream.Flush();
            stream.Close();
        }



        public string SaveFile(string filename)
        {
            string ext = Path.GetExtension(filename).ToUpper();
            switch (ext)
            {
                case ".PDF":
                    SaveFilePDF(filename);
                    return ".PDF";
                case ".XPS":
                    SaveFileXPS(filename);
                    return ".XPS";
                case ".XAML":
                default:
                    SaveFileXAML(filename);
                    break;
            }
            return ".XAML";
        }

        public void SaveFileXAML(string file)
        {
            using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                TextRange textRange = new TextRange(Document.ContentStart, Document.ContentEnd);
                textRange.Save(fs, DataFormats.XamlPackage);
            }
        }


        public void SaveFilePDF(string fileName)
        {
            FlowDocument fd = richTextBox.Document;
            double pw = richTextBox.Document.PageWidth;

            MemoryStream lMemoryStream = new MemoryStream();

            fd.PageWidth = PrintLayout.A4Narrow.Size.Width;
            fd.ColumnWidth = PrintLayout.A4Narrow.ColumnWidth;
            fd.PageHeight = PrintLayout.A4Narrow.Size.Height;
            fd.PagePadding = PrintLayout.A4Narrow.Margin;

            using (Package container = Package.Open(lMemoryStream, FileMode.Create))
            {
                using (XpsDocument xpsDoc = new XpsDocument(container, CompressionOption.Maximum))
                {
                    XpsSerializationManager rsm = new XpsSerializationManager(new XpsPackagingPolicy(xpsDoc), false);
                    var paginator = ((IDocumentPaginatorSource)richTextBox.Document).DocumentPaginator;
                    rsm.SaveAsXaml(paginator);
                }
            }

            fd.PageWidth = pw;
            fd.PagePadding = PAGE_PADDING;

            PdfSharp.Xps.XpsModel.XpsDocument pdfXpsDoc = PdfSharp.Xps.XpsModel.XpsDocument.Open(lMemoryStream);
            PdfSharp.Xps.XpsConverter.Convert(pdfXpsDoc, fileName);
        }

        public void SaveFileXPS(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);


            FlowDocument fd = richTextBox.Document;
            double pw = richTextBox.Document.PageWidth;

            fd.PageWidth = PrintLayout.A4Narrow.Size.Width;
            fd.ColumnWidth = PrintLayout.A4Narrow.ColumnWidth;
            fd.PageHeight = PrintLayout.A4Narrow.Size.Height;
            fd.PagePadding = PrintLayout.A4Narrow.Margin;

            using (Package container = Package.Open(fileName, FileMode.Create))
            {
                using (XpsDocument xpsDoc = new XpsDocument(container, CompressionOption.Maximum))
                {
                    XpsSerializationManager rsm = new XpsSerializationManager(new XpsPackagingPolicy(xpsDoc), false);
                    var paginator = ((IDocumentPaginatorSource)richTextBox.Document).DocumentPaginator;
                    rsm.SaveAsXaml(paginator);
                }
            }

            fd.PageWidth = pw;
            fd.PagePadding = PAGE_PADDING;
        }



        public void Load(string XAML)
        {
            StringReader stringReader = new StringReader(XAML);
            System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(stringReader);
            var v = System.Windows.Markup.XamlReader.Load(xmlReader);
            if (v is FlowDocument)
            {
                richTextBox.Document = v as FlowDocument;
            }
            else if (v is Section)
            {
                Section sec = v as Section;
                FlowDocument doc = new FlowDocument();
                while (sec.Blocks.Count > 0)
                    doc.Blocks.Add(sec.Blocks.FirstBlock);
                richTextBox.Document = doc;
            }
        }

        public object LoadXAMLElement(string value)
        {
            return LoadXAMLElement(
                XmlReader.Create(new StringReader(value)));
        }

        public object LoadXAMLElement(Stream stream)
        {
            return System.Windows.Markup.XamlReader.Load(stream);
        }

        public object LoadXAMLElement(XmlReader stream)
        {
            return System.Windows.Markup.XamlReader.Load(stream);
        }

        public string XAML
        {
            get
            {
                return System.Windows.Markup.XamlWriter.Save(richTextBox.Document);
            }

            set
            {
                var v = LoadXAMLElement(value);
                if (v is FlowDocument)
                {
                    richTextBox.Document = v as FlowDocument;
                }
                else if (v is Section)
                {
                    Section sec = v as Section;
                    FlowDocument doc = new FlowDocument();
                    while (sec.Blocks.Count > 0)
                        doc.Blocks.Add(sec.Blocks.FirstBlock);
                    richTextBox.Document = doc;
                }
            }
        }

        public string PlainText
        {
            get
            {
                TextRange tr = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                return tr.Text;
            }
        }

        private byte[] SaveAllContent(RichTextBox richTextBox)
        {
            var content = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            using (MemoryStream ms = new MemoryStream())
            {
                content.Save(ms, DataFormats.XamlPackage, true);
                return ms.ToArray();
            }
        }

        private void LoadAllContent(byte[] bd, RichTextBox richTextBox)
        {
            var content = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            MemoryStream ms = new MemoryStream(bd);
            content.Load(ms, System.Windows.DataFormats.XamlPackage);
        }

        public byte[] ComproessedDocument
        {
            get
            {
                return SaveAllContent(richTextBox);
            }

            set
            {
                LoadAllContent(value, richTextBox);
            }
        }

        public bool isTable
        {
            get
            {
                return TryFindParent<Table>(richTextBox.CaretPosition.Parent as DependencyObject) != null;
            }
        }

        public Thickness TableBorderTickness
        {
            get
            {
                Thickness tick = new Thickness(0);
                Table t;
                List<TableCell> ltc = GetSelectedCells(richTextBox.Selection, out t);
                if (ltc.Count <= 0) return tick;

                tick = ltc[0].BorderThickness;
                foreach (TableCell tc in ltc)
                {
                    if (tc.BorderThickness.Left != tick.Left)
                    {
                        tick.Left = 0;
                    }
                    if (tc.BorderThickness.Right != tick.Right)
                    {
                        tick.Right = 0;
                    }

                    if (tc.BorderThickness.Top != tick.Top)
                    {
                        tick.Top = 0;
                    }
                    if (tc.BorderThickness.Bottom != tick.Bottom)
                    {
                        tick.Bottom = 0;
                    }
                }

                return tick;
            }
        }





        public bool isBold
        {
            get
            {
                return (SelectionFontWeight == FontWeights.Bold);
            }
        }


        public bool isItalic
        {
            get
            {
                return (SelectionFontStyle == FontStyles.Italic);
            }
        }

        public bool isUnderline
        {
            get
            {
                TextDecorationCollection tdc = this.SelectionTextDecoration;
                if (tdc == null) return false;
                return tdc.Contains(TextDecorations.Underline[0]);
            }
        }

        public void CloneRun(Run source, Run dest)
        {
            dest.FontFamily = source.FontFamily;
            dest.FontSize = source.FontSize;
            dest.FontWeight = source.FontWeight;
            dest.Foreground = source.Foreground;
            dest.Background = source.Background;
            dest.TextDecorations = source.TextDecorations;
        }

        public static double PointsToPixels(double points)
        {
            return points * (96.0 / 72.0);
        }

        public static double PixelsToPoints(double pixels)
        {
            return pixels * (72.0 / 96.0);
        }


        public string SelectionFontSizePoints
        {
            get
            {
                object n = richTextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);
                if (n == DependencyProperty.UnsetValue)
                {
                    return "12";
                }
                double v = Convert.ToDouble(n);
                return Convert.ToString((int)Math.Round(PixelsToPoints(v)));
            }
            set
            {
                Console.WriteLine(value);
                int v = (int)Math.Round(PointsToPixels(Convert.ToDouble(value)));
                Console.WriteLine(v);
                Console.WriteLine(Convert.ToString(v));
                richTextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty,Convert.ToString(v));
            }
        }

        public string SelectionFontSize
        {
            get
            {
                object n = richTextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);
                if (n == DependencyProperty.UnsetValue)
                {
                    return "12";
                }
                return (string)n;
            }
            set
            {
                richTextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, value);
            }
        }

        public FontFamily SelectionFontFamily
        {
            get
            {
                object n = richTextBox.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
                if (n == DependencyProperty.UnsetValue)
                {
                    return null;
                }
                FontFamily ff = (FontFamily)n;

                //string[] fn = ff.FamilyNames;
                return ff;

            }
            set
            {
                richTextBox.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, value);

            }
        }

        public FontWeight SelectionFontWeight
        {
            get
            {
                object n = richTextBox.Selection.GetPropertyValue(TextElement.FontWeightProperty);
                if (n == DependencyProperty.UnsetValue)
                {
                    return FontWeights.Normal;
                }
                return (FontWeight)n;
            }
            set
            {
                //if (value == SelectionFontWeight) return;
                //if (richTextBox.Selection.IsEmpty)
                //{
                //    DependencyObject se = richTextBox.Selection.Start.Parent;
                //    Run r = new Run("", richTextBox.Selection.End);

                //    if (se is Run)
                //    {
                //        CloneRun((se as Run), r);
                //    }
                //    r.FontWeight = value;
                //}
                //else
                //{
                //}
                richTextBox.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, value);
            }
        }

        public TextDecorationCollection SelectionTextDecoration
        {
            get
            {
                object n = richTextBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty) as TextDecorationCollection;
                if (n == DependencyProperty.UnsetValue)
                {
                    return null;
                }
                return (TextDecorationCollection)n;
            }
        }


        public Brush SelectionBackground
        {
            get
            {
                object n = richTextBox.Selection.GetPropertyValue(TextElement.BackgroundProperty);
                if (n == DependencyProperty.UnsetValue)
                {
                    return null;
                }
                return (Brush)n;
            }
            set
            {
                //if (richTextBox.Selection.IsEmpty)
                //{
                //    DependencyObject se = richTextBox.Selection.Start.Parent;
                //    Run r = new Run("", richTextBox.Selection.End);

                //    if (se is Run)
                //    {
                //        CloneRun((se as Run), r);
                //    }
                //    r.Background = value;
                //}
                //else
                //{

                //}

                richTextBox.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, value);
            }
        }

        public Brush SelectionForeground
        {
            get
            {
                object n = richTextBox.Selection.GetPropertyValue(TextElement.ForegroundProperty);
                if (n == DependencyProperty.UnsetValue)
                {
                    return null;
                }
                return (Brush)n;
            }
            set
            {
                //if (richTextBox.Selection.IsEmpty)
                //{
                //    DependencyObject se = richTextBox.Selection.Start.Parent;
                //    Run r = new Run("", richTextBox.Selection.End);

                //    if (se is Run)
                //    {
                //        CloneRun((se as Run), r);
                //    }
                //    r.Foreground = value;
                //}
                //else
                //{

                //}

                richTextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, value);
            }
        }




        public TextMarkerStyle SelectionMarkerStyle
        {
            get
            {
                Paragraph startParagraph = richTextBox.Selection.Start.Paragraph;
                Paragraph endParagraph = richTextBox.Selection.End.Paragraph;
                if (startParagraph != null && endParagraph != null && (startParagraph.Parent is ListItem) && (endParagraph.Parent is ListItem) && object.ReferenceEquals(((ListItem)startParagraph.Parent).List, ((ListItem)endParagraph.Parent).List))
                {
                    TextMarkerStyle markerStyle = ((ListItem)startParagraph.Parent).List.MarkerStyle;
                    return markerStyle;
                }
                return TextMarkerStyle.None;
            }

        }

        public FontStyle SelectionFontStyle
        {
            get
            {
                object n = richTextBox.Selection.GetPropertyValue(TextElement.FontStyleProperty);
                if (n == DependencyProperty.UnsetValue)
                {
                    return FontStyles.Normal;
                }
                return (FontStyle)n;
            }
            set
            {
                //if (value == SelectionFontStyle) return;
                //if (richTextBox.Selection.IsEmpty)
                //{
                //    DependencyObject se = richTextBox.Selection.Start.Parent;
                //    Run r = new Run("", richTextBox.Selection.End);

                //    if (se is Run)
                //    {
                //        CloneRun((se as Run), r);
                //    }
                //    r.FontStyle = value;
                //}
                //else
                //{
                //}

                richTextBox.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, value);
            }
        }


        public TextAlignment SelectionAligment
        {
            get
            {
                object n = richTextBox.Selection.GetPropertyValue(Paragraph.TextAlignmentProperty);
                if (n == DependencyProperty.UnsetValue)
                {
                    return TextAlignment.Left;
                }
                return (TextAlignment)n;
            }
            set
            {
                switch (value)
                {
                    case TextAlignment.Left:
                        cmdExecute(EditingCommands.AlignLeft);
                        return;
                    case TextAlignment.Center:
                        cmdExecute(EditingCommands.AlignCenter);
                        return;
                    case TextAlignment.Right:
                        cmdExecute(EditingCommands.AlignRight);
                        return;
                    case TextAlignment.Justify:
                        cmdExecute(EditingCommands.AlignJustify);
                        return;
                }
            }
        }



        public double PageWidth
        {
            get { return richTextBox.Document.PageWidth; }
            set { richTextBox.Document.PageWidth = value; }
        }


        private void richTextBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            TextPointer tp = richTextBox.GetPositionFromPoint(e.GetPosition(richTextBox), false);
            if (tp != null && tp.Parent is DependencyObject)
            {
                Hyperlink h = TryFindParent<Hyperlink>(tp.Parent as DependencyObject);
                if (h != null && h.NavigateUri != null)
                {
                    Process.Start(new ProcessStartInfo(h.NavigateUri.AbsoluteUri));
                }
            }
        }

        private void richTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextPointer tp = richTextBox.GetPositionFromPoint(e.GetPosition(richTextBox), false);
            if (tp == null)
            {
                image_helper.ClearImageResizers(richTextBox);
                return;
            }

            if (tp.Parent is InlineUIContainer)
            {
                UIElement ue = (tp.Parent as InlineUIContainer).Child;
                if (ue == null) return;

                if (ue is Image)
                {
                    cmdSelect((tp.Parent as InlineUIContainer));
                    image_helper.ChangeImageResizers(richTextBox, (ue as Image));
                    e.Handled = true;
                    return;
                }
            }

            if (tp.Parent is BlockUIContainer)
            {
                // Get the BlockUIContainer or InlilneUIContainer's  full
                // XAML markup.

                UIElement ue = (tp.Parent as BlockUIContainer).Child;
                if (ue == null) return;

                if (ue is Image)
                {
                    cmdSelect((tp.Parent as BlockUIContainer));

                    image_helper.ChangeImageResizers(richTextBox, (ue as Image));
                    e.Handled = true;
                    return;
                }
            }
            image_helper.ClearImageResizers(richTextBox);
        }

        #region Paste

        private bool _isBusy = false;
        public bool isBusy
        {
            get { return _isBusy; }
            set
            {
                if (value == _isBusy) return;
                _isBusy = value;
                if (_isBusy)
                {
                    progressGrid.Visibility = Visibility.Visible;
                    // richTextBox.IsEnabled = false;
                }
                else
                {

                    progressGrid.Visibility = Visibility.Collapsed;
                }

            }
        }

        public new bool Focus()
        {
            return richTextBox.Focus();
        }

        /*Paste data like HTML*/
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            string[] formats = e.DataObject.GetFormats();

            for (int i = formats.Length - 1; i >= 0; i--)
            {
                Console.WriteLine(formats[i]);
                Console.WriteLine(e.DataObject.GetDataPresent(DataFormats.Html) + "\n\n\n\n\n\n");
            }

            for (int i = formats.Length - 1; i >= 0; i--)
            {
                if (e.DataObject.GetDataPresent(DataFormats.Html))
                {

                    string html = (string)e.DataObject.GetData(DataFormats.Html);
                    Section s = HTMLConverter.HTMLToFlowConverter.ConvertHtmlToSection(html, richTextBox.Document.PageWidth);
                    this.cmdInsertBlock(s, true);

                    HyperlinkHelper.SubscribeToAllHyperlinks(richTextBox.Document);
                    e.CancelCommand();
                    e.Handled = true;


                    return;
                }
                string str = formats[i];
                switch (str)
                {
                    //case "HTML Format":
                    //    if (e.DataObject.GetDataPresent(DataFormats.Html))
                    //    {

                    //            string html = (string)e.DataObject.GetData(DataFormats.Html);
                    //            Section s = HTMLConverter.HTMLToFlowConverter.ConvertHtmlToSection(html, richTextBox.Document.PageWidth);
                    //            this.cmdInsertBlock(s,true);

                    //            Semagsoft.HyperlinkHelper.SubscribeToAllHyperlinks(richTextBox.Document);
                    //            e.CancelCommand();
                    //            e.Handled = true;


                    //        return;
                    //    }
                    //    break;
                    case "Rich Text Format":
                        if (e.DataObject.GetDataPresent(DataFormats.Rtf))
                        {

                            object o = e.DataObject.GetData(DataFormats.Rtf);
                            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(o as string));
                            richTextBox.Selection.Load(ms, DataFormats.Rtf);
                            HyperlinkHelper.SubscribeToAllHyperlinks(richTextBox.Document);


                            Block b = null; Section s = null;
                            TextPointer insert = richTextBox.Selection.Start;
                            b = TryFindParent<Block>(insert.Parent as DependencyObject);
                            s = TryFindParent<Section>(insert.Parent as DependencyObject);
                            Paragraph newItem = new Paragraph(new Run(""));
                            if (s == null)
                            {
                                if (b != null) this.Document.Blocks.InsertAfter(b, newItem);
                                else this.Document.Blocks.Add(newItem);
                            }
                            else
                            {
                                if (b != null) s.Blocks.InsertAfter(b, newItem);
                                else s.Blocks.Add(newItem);
                            }
                            richTextBox.Selection.Select(newItem.ContentStart, newItem.ContentStart);

                            e.CancelCommand();
                            e.Handled = true;
                            return;
                        }
                        break;
                    case "DeviceIndependentBitmap":
                    case "System.Windows.Media.Imaging.BitmapSource":
                    case "System.Drawing.Bitmap":
                    case "Bitmap":
                        if (e.DataObject.GetDataPresent(DataFormats.Bitmap))
                        {

                            BitmapSource image = Clipboard.GetImage();
                            // = (BitmapSource)e.DataObject.GetData(DataFormats.Bitmap);
                            cmdInsertImage(image, ImageContentType.ImageJpegContentType);
                            e.CancelCommand();
                            e.Handled = true;

                            return;
                        }
                        break;
                    case "Text":
                        if (e.DataObject.GetDataPresent(DataFormats.Text))
                        {
                            string text = (string)e.DataObject.GetData(DataFormats.Text);
                            Uri uriResult;
                            string original = text;
                            if (YanaHelper.IsHyperlink(ref text, out uriResult))
                            {

                                Hyperlink h = new Hyperlink(richTextBox.Selection.Start, richTextBox.Selection.End);
                                h.Cursor = Cursors.Hand;
                                h.NavigateUri = uriResult;
                                h.Inlines.Add(new Run(original));
                                h.FontSize = 16;
                                // this.cmdInsertInline(h);

                            }
                            else
                            {
                                this.cmdInsertText(text);
                            }
                            e.CancelCommand();
                            e.Handled = true;


                            return;
                        }
                        break;
                }
            }
            #endregion Paste
        }

        public event ExecutedRoutedEventHandler PreviewCommandExecuted;


        private void richTextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (PreviewCommandExecuted != null)
            {
                PreviewCommandExecuted.Invoke(sender, e);
            }
        }

        private void richTextBox_ScrollChanged(object sender, RoutedEventArgs e)
        {
            UpdateAdorners();
        }

        #endregion

        private void richTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (updateRTB)
            {
                updateUI = false;
                if (GetBindingExpression(HtmlProperty) != null)
                {
                    string xaml = System.Windows.Markup.XamlWriter.Save(Editer.Document);
                    Console.WriteLine(xaml);
                    Html = HtmlFromXamlConverter.ConvertXamlToHtml(xaml);
                }
                if (GetBindingExpression(TextProperty) != null)
                {
                    TextRange textRange = new TextRange(Editer.Document.ContentStart, Editer.Document.ContentEnd);
                    Text = textRange.Text;
                }
                updateUI = true;
            }
        }

        private void richTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            updateRTB = true;
            //updateUI = false;
            //if (GetBindingExpression(HtmlProperty) != null)
            //{
            //    string xaml = System.Windows.Markup.XamlWriter.Save(Editer.Document);
            //    Html = HtmlFromXamlConverter.ConvertXamlToHtml(xaml);
            //}
            //if (GetBindingExpression(TextProperty) != null)
            //{
            //    TextRange textRange = new TextRange(Editer.Document.ContentStart, Editer.Document.ContentEnd);
            //    Text = textRange.Text;
            //}
            //updateUI = true;
        }
    }

    public class fontsize
    {
        /// <summary>
        /// 属性的模板写法
        /// </summary>
        private int _id;
        public int Id
        {
            get => _id;
            set { _id = value; }
        }

        /// <summary>
        /// 属性的模板写法
        /// </summary>
        public string Name
        {
            get => Id.ToString();
        }
    }
}
