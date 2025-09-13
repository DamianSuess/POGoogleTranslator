using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using POParser;
using Karambolo.PO;

public class MainWindow : Window
{
  private VBox vbox;
  private MenuBar menubar;
  private Menu fileMenu;
  private MenuItem file;
  private MenuItem openItem;
  private MenuItem saveItem;

  private Toolbar toolbar;
  private ToggleToolButton togglePanelButton;
  private ScrolledWindow scroll;
  private TreeView treeView;
  private ListStore listStore;

  // Bottom panel
  private EventBox bottomPanel;
  private int bottomPanelHeight = 100;

  // Split panel and text boxes
  private HPaned hpaned;
  private TextView sourceTextView;
  private TextView translationTextView;

  public MainWindow() : base("PO Editor")
  {
    vbox = new VBox();
    menubar = new MenuBar();
    fileMenu = new Menu();
    file = new MenuItem("File");
    openItem = new MenuItem("Open");
    saveItem = new MenuItem("Save");

    toolbar = new Toolbar();
    togglePanelButton = new ToggleToolButton(Gtk.Stock.GoDown);
    scroll = new ScrolledWindow();
    treeView = new TreeView();
    listStore = new ListStore(typeof(string), typeof(string));

    // Bottom panel
    bottomPanel = new EventBox();
    bottomPanelHeight = 100;

    // Split panel and text boxes
    hpaned = new HPaned();
    sourceTextView = new TextView();
    translationTextView = new TextView();

    InitializeComponents();
  }

  private void InitializeComponents()
  {
    SetDefaultSize(800, 600);
    DeleteEvent += (o, args) => Application.Quit();
    Add(vbox);

    // Menu
    file.Submenu = fileMenu;
    fileMenu.Append(openItem);
    fileMenu.Append(saveItem);
    menubar.Append(file);
    vbox.PackStart(menubar, false, false, 0u);

    // Toolbar
    toolbar.ToolbarStyle = ToolbarStyle.Icons;

    toolbar.Insert(togglePanelButton, -1);
    vbox.PackStart(toolbar, false, false, 0u);

    // TreeView
    treeView.Model = listStore;

    AppendColumn("Source (msgid)", 0, false, true);
    AppendColumn("Translation (msgstr)", 1, false, false);

    scroll.Add(treeView);
    vbox.PackStart(scroll, true, true, 0u);

    // Bottom panel setup (now with split and text boxes)
    hpaned.Pack1(sourceTextView, true, true);
    hpaned.Pack2(translationTextView, true, false);
    bottomPanel.Add(hpaned);
    bottomPanel.Visible = false;
    vbox.PackStart(bottomPanel, true, true, (uint)bottomPanelHeight);

    // Toggle button setup
    togglePanelButton.Active = true;
    togglePanelButton.Toggled += (sender, e) =>
    {
        bottomPanel.Visible = togglePanelButton.Active;
        togglePanelButton.Label = togglePanelButton.Active ? "Hide Panel" : "Show Panel";
    };

    // Events
    openItem.Activated += (sender, e) =>
    {
        var dialog = new FileChooserDialog("Open PO File", this, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
        if (dialog.Run() == (int)ResponseType.Accept)
        {
            var data = parseCatalog(dialog.Filename);
            listStore.Clear();
            foreach (var item in data.Catalog)
            {
                if (item is POSingularEntry pse)
                {
                    listStore.AppendValues(item.Key.Id, item[0]);
                }
                else
                {
                    Console.WriteLine($"Only singular entries are supported. Key {item.Key.Id} skipped.");
                }
            }
        }
        dialog.Destroy();
    };

    saveItem.Activated += (sender, e) =>
    {
        var dialog = new FileChooserDialog("Save PO File", this, FileChooserAction.Save, "Cancel", ResponseType.Cancel, "Save", ResponseType.Accept);
        if (dialog.Run() == (int)ResponseType.Accept)
        {
            var rows = listStore.Cast<TreeIter>()
                .Select(iter => (
                    msgid: (string)listStore.GetValue(iter, 0),
                    msgstr: (string)listStore.GetValue(iter, 1)
                ))
                .ToList();

            var data = parseCatalog(dialog.Filename);
            foreach (var row in rows)
            {
                var entry = data.Catalog.FirstOrDefault(e => e.Key.Id == row.msgid);
                if (entry is POSingularEntry pse)
                {
                    pse.Translation = row.msgstr;
                }
            }
            writePOCatalog(dialog.Filename, data);
        }
        dialog.Destroy();
    };
  }

  private void AppendColumn(string title, int colIndex, bool editable, bool resizable)
  {
    var renderer = new CellRendererText();
    renderer.Height = 24;
    renderer.Ellipsize = Pango.EllipsizeMode.End;
    var col = new TreeViewColumn(title, renderer);
    col.Resizable = resizable;
    if (editable)
    {
      renderer.Editable = true;
      renderer.Edited += (o, args) =>
      {
        TreeIter iter;
        if (listStore.GetIterFromString(out iter, args.Path))
        {
          listStore.SetValue(iter, colIndex, args.NewText);
        }
      };
    }
    
    col.PackStart(renderer, true);
    col.AddAttribute(renderer, "text", colIndex);
    if (colIndex == 0)
    {
      col.MaxWidth = 400;
    }

    treeView.AppendColumn(col);
  }
}
