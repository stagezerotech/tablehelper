using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;

namespace StageZero.TableHelper
{
    public class Table<TItem>
    {
        static string SearchByParam;
        static string SearchColParam;
        static string SortByParam;
        static string SortOrderParam;
        static string SortOrderAsc;
        static string SortOrderDesc;

        static string PageTakeParam;
        static string PageSkipParam;

        class Column
        {
            public int Index { get; set; }

            public string Name { get; set; }

            public Func<TItem, object> DisplayExpression { get; set; }

            public Expression<Func<TItem, object>> SortExpression { get; set; }

            public Expression<Func<TItem, bool>> SearchExpression { get; set; }

            public IList<ColumnSetting> ColumnSettings { get; set; }

            public Func<TItem, object> CellAttributesFunction = null;
        }

        IQueryable<TItem> Items { get; set; }
        IList<Column> Columns { get; set; } = new List<Column>();

        IHtmlHelper HtmlHelper { get; set; }

        object HtmlAttributes { get; set; }

        /// <summary>
        /// Use this property in the SearchExpression
        /// </summary>
        public string SearchTerm { get; private set; }

        /// <summary>
        /// Criteria for when search will be applied. Default is "when not null or empty".
        /// </summary>
        public Func<string, bool> SearchTermCriteria { get; set; } = s => !string.IsNullOrEmpty(s);

        /// <summary>
        /// Options for size of page. Default are 10, 20, 50 and 100
        /// </summary>
        public int[] PageSizeOptions = { 10, 20, 50, 100 };

        /// <summary>
        /// Starting value for page size. Is set to 20 by default.
        /// </summary>
        public int DefaultTakeItems = 20;

        /// <summary>
        /// Starting value for items to skip. Set to 0 by default.
        /// </summary>
        public int DefaultSkipItems = 0;

        /// <summary>
        /// Placement of pagination. Options are Top, Bottom or Both, with Bottom as default.
        /// </summary>
        public ElementPlacement PaginationPlacement = ElementPlacement.Bottom;

        /// <summary>
        /// Alignment of pagination. Options are Left, Center and Right, with Right as default.
        /// </summary>
        public ElementAlignment PaginationAlignment = ElementAlignment.Right;

        /// <summary>
        /// Text to show on pagination button for first page. Default is &lt;&lt;.
        /// </summary>
        public string FirstPageText = "<<";

        /// <summary>
        /// Text to show on pagination button for last page. Default is &gt;&gt;.
        /// </summary>
        public string LastPageText = ">>";

        /// <summary>
        /// Character to show if column is sorted by ascending. Default is ▲.
        /// </summary>
        public string AscendingCharacter = "▲";

        /// <summary>
        /// Character to show if column is sorted by descending. Default is ▼.
        /// </summary>
        public string DescendingCharacter = "▼";

        /// <summary>
        /// Placement of search element. Options are Top, Bottom or Both, with Top as default.
        /// </summary>
        public ElementPlacement SearchPlacement = ElementPlacement.Top;

        /// <summary>
        /// Alignment of search element. Options are Left, Center and Right, with Right as default.
        /// </summary>
        public ElementAlignment SearchAlignment = ElementAlignment.Right;

        /// <summary>
        /// Text to show on search button. Default is 🔍.
        /// </summary>
        public string SearchButtonText = "🔍";

        /// <summary>
        /// Text to show on search clear button. Default is ×.
        /// </summary>
        public string SearchClearText = "×";

        /// <summary>
        /// Gives the row attributes based on the item
        /// </summary>
        public Func<TItem, object> RowAttributesFunction = null;

        internal Table(IHtmlHelper htmlHelper, IQueryable<TItem> items, string id, object htmlAttributes)
        {
            HtmlHelper = htmlHelper;
            Items = items;
            HtmlAttributes = htmlAttributes;

            var query = htmlHelper.ViewContext.HttpContext.Request.Query;
            SearchTerm = query.ContainsKey(SearchByParam) ? query[SearchByParam][0] : null;

            var prefix = id == null ? null : id + "-";
            SearchByParam = $"{prefix}searchBy";
            SearchColParam = $"{prefix}searchCol";
            SortByParam = $"{prefix}sortBy";
            SortOrderParam = $"{prefix}sortOrder";
            SortOrderAsc = $"{prefix}asc";
            SortOrderDesc = $"{prefix}desc";
            PageTakeParam = $"{prefix}take";
            PageSkipParam = $"{prefix}skip";
        }

        /// <summary>
        /// Add a column to the table.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        /// <param name="displayExpression">Expression by which to render text to display in column cell</param>
        /// <param name="columnSettings">Special settings for column</param>
        public void AddCol(string columnName,
            Func<TItem, object> displayExpression,
            params ColumnSetting[] columnSettings)
        {
            AddCol(columnName, displayExpression, null, null, null, columnSettings);
        }

        /// <summary>
        /// Add a column to the table.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        /// <param name="displayExpression">Expression by which to render text to display in column cell</param>
        /// <param name="searchExpression">Expression by which to perform search on column. Use <see cref="SearchTerm"/> as the search term.</param>
        /// <param name="columnSettings">Special settings for column</param>
        public void AddCol(string columnName,
            Func<TItem, object> displayExpression,
            Expression<Func<TItem, bool>> searchExpression,
            params ColumnSetting[] columnSettings)
        {
            AddCol(columnName, displayExpression, null, searchExpression, null, columnSettings);
        }

        /// <summary>
        /// Add a column to the table.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        /// <param name="displayExpression">Expression by which to render text to display in column cell</param>
        /// <param name="sortExpression">Expression by which to sort the column.</param>
        /// <param name="columnSettings">Special settings for column</param>
        public void AddCol(string columnName,
            Func<TItem, object> displayExpression,
            Expression<Func<TItem, object>> sortExpression,
            params ColumnSetting[] columnSettings)
        {
            AddCol(columnName, displayExpression, sortExpression, null, null, columnSettings);
        }

        /// <summary>
        /// Add a column to the table.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        /// <param name="displayExpression">Expression by which to render text to display in column cell</param>
        /// <param name="sortExpression">Expression by which to sort the column.</param>
        /// <param name="searchExpression">Expression by which to perform search on column. Use <see cref="SearchTerm"/> as the search term.</param>
        /// <param name="cellAttributesFunction">Gives the cell attributes based on the item</param>
        /// <param name="columnSettings">Special settings for column</param>
        public void AddCol(string columnName,
            Func<TItem, object> displayExpression,
            Expression<Func<TItem, object>> sortExpression = null,
            Expression<Func<TItem, bool>> searchExpression = null,
            Func<TItem, object> cellAttributesFunction = null,
            params ColumnSetting[] columnSettings)
        {
            var colSetting = new Column
            {
                Index = Columns.Count(),
                Name = columnName,
                DisplayExpression = displayExpression,
                SortExpression = sortExpression,
                SearchExpression = searchExpression,
                CellAttributesFunction = cellAttributesFunction,
                ColumnSettings = columnSettings.ToList()
            };

            Columns.Add(colSetting);
        }

        internal IHtmlContent Render()
        {
            var shownCount = ApplyParameters();

            var items = Items.ToList();

            var tableHolderBuilder = new TagBuilder("div");

            var searchPanel = GenerateSearchPanel();

            if (searchPanel != null && SearchPlacement == ElementPlacement.Top || SearchPlacement == ElementPlacement.Both)
                tableHolderBuilder.InnerHtml.AppendHtml(searchPanel);

            var pagination = GeneratePagination(shownCount);

            if (pagination != null && PaginationPlacement == ElementPlacement.Top || PaginationPlacement == ElementPlacement.Both)
                tableHolderBuilder.InnerHtml.AppendHtml(pagination);

            var tableBuilder = new TagBuilder("table");
            tableBuilder.AddCssClass("table");

            if (HtmlAttributes != null)
                tableBuilder.MergeAttributes(new RouteValueDictionary(HtmlAttributes));

            tableBuilder.InnerHtml.AppendHtml($"<thead><tr>{string.Join("", Columns.Select(c => $"<th>{GenerateColumnHead(c)}</th>"))}</tr></thead>");

            var bodyBuilder = new TagBuilder("tbody");

            foreach (var item in items)
            {
                var rowBuilder = new TagBuilder("tr");

                if (RowAttributesFunction != null)
                    rowBuilder.MergeAttributes(new RouteValueDictionary(RowAttributesFunction(item)));

                foreach (var col in Columns)
                {
                    var cellBuilder = new TagBuilder("td");

                    if (col.CellAttributesFunction != null)
                        cellBuilder.MergeAttributes(new RouteValueDictionary(col.CellAttributesFunction(item)));

                    var display = col.DisplayExpression(item);

                    if (display is IHtmlContent content)
                        cellBuilder.InnerHtml.AppendHtml(content);
                    else
                        cellBuilder.InnerHtml.Append(display != null ? display.ToString() : String.Empty);

                    rowBuilder.InnerHtml.AppendHtml(cellBuilder);
                }

                bodyBuilder.InnerHtml.AppendHtml(rowBuilder);
            }

            tableBuilder.InnerHtml.AppendHtml(bodyBuilder);

            tableHolderBuilder.InnerHtml.AppendHtml(tableBuilder);

            if (pagination != null && PaginationPlacement == ElementPlacement.Bottom || PaginationPlacement == ElementPlacement.Both)
                tableHolderBuilder.InnerHtml.AppendHtml(pagination);

            if (searchPanel != null && SearchPlacement == ElementPlacement.Bottom || SearchPlacement == ElementPlacement.Both)
                tableHolderBuilder.InnerHtml.AppendHtml(searchPanel);

            return tableHolderBuilder;
        }

        private int ApplyParameters()
        {
            var request = HtmlHelper.ViewContext.HttpContext.Request;

            ApplySearch(request);

            var shownCount = Items.Count();

            ApplySort(request);
            ApplyPagination(request);

            return shownCount;
        }

        private void ApplySearch(HttpRequest request)
        {
            if (request.Query.ContainsKey(SearchByParam) && SearchTermCriteria(SearchTerm))
            {
                var colParam = request.Query[SearchColParam];

                if (!colParam.Any() || !int.TryParse(colParam[0], out int index))
                    return;

                var column = Columns.SingleOrDefault(c => c.Index == index);

                if (column == null)
                    return;

                Items = Items.Where(column.SearchExpression);
            }
        }

        private void ApplySort(HttpRequest request)
        {
            if (!Columns.Any())
                return;

            var ascending = true;
            var column = Columns.SingleOrDefault(c => c.ColumnSettings.Any(cs => cs == ColumnSetting.DefaultSort)) ?? Columns.First();

            if (request.Query.ContainsKey(SortByParam))
            {
                var colParam = request.Query[SortByParam];

                if (colParam.Any() && int.TryParse(colParam[0], out int index))
                {
                    column = Columns.SingleOrDefault(c => c.Index == index) ?? column;
                }

                ascending = !(request.Query.ContainsKey(SortOrderParam) && request.Query[SortOrderParam][0] == SortOrderDesc);
            }
            else
            {
                ascending = !column.ColumnSettings.Any(c => c == ColumnSetting.FirstSortDesc);
            }

            if (ascending)
                Items = Items.OrderBy(column.SortExpression);
            else
                Items = Items.OrderByDescending(column.SortExpression);
        }

        private void ApplyPagination(HttpRequest request)
        {
            if (!request.Query.ContainsKey(PageTakeParam) || !int.TryParse(request.Query[PageTakeParam][0], out int take))
                take = DefaultTakeItems;

            if (!request.Query.ContainsKey(PageSkipParam) || !int.TryParse(request.Query[PageSkipParam][0], out int skip))
                skip = DefaultSkipItems;

            Items = Items.Skip(skip).Take(take);
        }

        private string GenerateColumnHead(Column col)
        {
            if (col.SortExpression == null)
                return col.Name;

            var request = HtmlHelper.ViewContext.HttpContext.Request;

            int? urlIndex = null;
            bool? urlOrderAsc = null;

            if (request.Query.ContainsKey(SortByParam) && int.TryParse(request.Query[SortByParam][0], out int index))
                urlIndex = index;

            if (request.Query.ContainsKey(SortOrderParam))
                urlOrderAsc = request.Query[SortOrderParam][0] == SortOrderAsc;

            var parameters = new Dictionary<string, object>
            {
                { SortByParam, col.Index },
                { SortOrderParam, GetLinkSortOrder(col, urlIndex, urlOrderAsc) }
            };

            var sortUrl = SetUrlParameters(request, parameters);

            return $"<a href='{sortUrl}'>{col.Name}{GetSortArrow(col, urlIndex, urlOrderAsc)}</a>";
        }

        private string GetSortArrow(Column col, int? urlIndex, bool? urlOrderAsc)
        {
            if ((urlIndex.HasValue && col.Index != urlIndex) ||
                (!urlIndex.HasValue && col.ColumnSettings.All(c => c != ColumnSetting.DefaultSort)))
                return "";

            if (urlOrderAsc.HasValue)
                return urlOrderAsc.Value ? AscendingCharacter : DescendingCharacter;

            return col.ColumnSettings.All(c => c != ColumnSetting.FirstSortDesc) ? AscendingCharacter : DescendingCharacter;
        }

        private string GetLinkSortOrder(Column col, int? urlIndex, bool? urlOrderAsc)
        {
            if ((urlIndex.HasValue && col.Index != urlIndex) ||
                (!urlIndex.HasValue && col.ColumnSettings.All(c => c != ColumnSetting.DefaultSort)))
                return col.ColumnSettings.All(c => c != ColumnSetting.FirstSortDesc) ? SortOrderAsc : SortOrderDesc;

            if (urlIndex.HasValue && col.Index == urlIndex)
                return !urlOrderAsc.HasValue || urlOrderAsc.Value ? SortOrderDesc : SortOrderAsc;

            return col.ColumnSettings.All(c => c != ColumnSetting.FirstSortDesc) ? SortOrderDesc : SortOrderAsc;
        }

        private IHtmlContent GenerateSearchPanel()
        {
            if (Columns.All(c => c.SearchExpression == null))
                return null;

            var searchFormBuilder = new TagBuilder("form");
            searchFormBuilder.AddCssClass("mb-2");
            searchFormBuilder.Attributes.Add("method", "GET");
            searchFormBuilder.Attributes.Add("action", SetUrlParameters(HtmlHelper.ViewContext.HttpContext.Request, new Dictionary<string, object>()));

            var searchPanelBuilder = new TagBuilder("div");
            searchPanelBuilder.AddCssClass($"row {GetElementAlignment(SearchAlignment)}");

            // Search column selector
            var searchSelectPanelBuilder = new TagBuilder("div");
            searchSelectPanelBuilder.AddCssClass("col-md-auto");

            var searchColumnSelectorBuilder = new TagBuilder("select");
            searchColumnSelectorBuilder.AddCssClass("form-control");
            searchColumnSelectorBuilder.Attributes.Add("name", SearchColParam);

            foreach (var col in Columns.Where(c => c.SearchExpression != null))
            {
                var optionBuilder = new TagBuilder("option");
                optionBuilder.Attributes.Add("value", col.Index.ToString());
                optionBuilder.InnerHtml.Append(col.Name);

                searchColumnSelectorBuilder.InnerHtml.AppendHtml(optionBuilder);
            }

            searchSelectPanelBuilder.InnerHtml.AppendHtml(searchColumnSelectorBuilder);
            searchPanelBuilder.InnerHtml.AppendHtml(searchSelectPanelBuilder);

            // Search term input
            var searchTermPanelBuilder = new TagBuilder("div");
            searchTermPanelBuilder.AddCssClass("col-md-auto");

            var searchFieldGroupBuilder = new TagBuilder("div");
            searchFieldGroupBuilder.AddCssClass("input-group");

            var searchFieldBuilder = new TagBuilder("input");
            searchFieldBuilder.AddCssClass("form-control");
            searchFieldBuilder.Attributes.Add("type", "text");
            searchFieldBuilder.Attributes.Add("value", SearchTerm);
            searchFieldBuilder.Attributes.Add("name", SearchByParam);

            searchFieldGroupBuilder.InnerHtml.AppendHtml(searchFieldBuilder);

            var searchFieldButtonGroupBuilder = new TagBuilder("div");
            searchFieldButtonGroupBuilder.AddCssClass("input-group-append");

            var searchClearBuilder = new TagBuilder("input");
            searchClearBuilder.AddCssClass("btn btn-warning");
            searchClearBuilder.Attributes.Add("type", "reset");
            searchClearBuilder.Attributes.Add("value", SearchClearText);

            searchFieldButtonGroupBuilder.InnerHtml.AppendHtml(searchClearBuilder);

            var searchButtonBuilder = new TagBuilder("button");
            searchButtonBuilder.AddCssClass("btn btn-primary");
            searchButtonBuilder.Attributes.Add("type", "submit");
            searchButtonBuilder.InnerHtml.AppendHtml(SearchButtonText);

            searchFieldButtonGroupBuilder.InnerHtml.AppendHtml(searchButtonBuilder);
            searchFieldGroupBuilder.InnerHtml.AppendHtml(searchFieldButtonGroupBuilder);
            searchTermPanelBuilder.InnerHtml.AppendHtml(searchFieldGroupBuilder);
            searchPanelBuilder.InnerHtml.AppendHtml(searchTermPanelBuilder);

            searchFormBuilder.InnerHtml.AppendHtml(searchPanelBuilder);

            return searchFormBuilder;
        }

        private IHtmlContent GeneratePagination(int total)
        {
            if (total == 0)
                return null;

            var request = HtmlHelper.ViewContext.HttpContext.Request;

            var taken = DefaultTakeItems;
            if (request.Query.ContainsKey(PageTakeParam) && int.TryParse(request.Query[PageTakeParam][0], out int urlTaken))
                taken = urlTaken;

            var skipped = 0;
            if (request.Query.ContainsKey(PageSkipParam) && int.TryParse(request.Query[PageSkipParam][0], out int urlSkipped))
                skipped = urlSkipped;

            var paginationPanelBuilder = new TagBuilder("div");
            paginationPanelBuilder.AddCssClass($"row {GetElementAlignment(PaginationAlignment)}");

            // Page size selector
            var paginationSelectPanelBuilder = new TagBuilder("div");
            paginationSelectPanelBuilder.AddCssClass("col-md-auto");

            var pageSizeSelectorBuilder = new TagBuilder("select");
            pageSizeSelectorBuilder.AddCssClass("form-control d-inline");
            pageSizeSelectorBuilder.Attributes.Add("onchange", "window.location=this.value;");

            foreach (var size in PageSizeOptions)
            {
                var optionBuilder = new TagBuilder("option");
                optionBuilder.InnerHtml.Append(size.ToString());
                optionBuilder.Attributes.Add("value", SetUrlParameters(request, new Dictionary<string, object> { { PageTakeParam, size } }));

                if (size == taken)
                    optionBuilder.Attributes.Add("selected", "selected");

                pageSizeSelectorBuilder.InnerHtml.AppendHtml(optionBuilder);
            }

            paginationSelectPanelBuilder.InnerHtml.AppendHtml(pageSizeSelectorBuilder);
            paginationPanelBuilder.InnerHtml.AppendHtml(paginationSelectPanelBuilder);

            // Pagination buttons
            var paginationButtonsPanelBuilder = new TagBuilder("div");
            paginationButtonsPanelBuilder.AddCssClass("col-md-auto");

            var paginationNavBuilder = new TagBuilder("nav");

            var paginationListBuilder = new TagBuilder("ul");
            paginationListBuilder.AddCssClass($"pagination {GetElementAlignment(PaginationAlignment)}");

            var firstUrl = SetUrlParameters(request, new Dictionary<string, object> { { PageSkipParam, 0 } });
            paginationListBuilder.InnerHtml.AppendHtml(GeneratePageLink(FirstPageText, firstUrl));

            for (int page = 0; page <= total / taken; page++)
            {
                if (page * taken < total)
                {
                    var isActive = page == skipped / taken;

                    var toSkip = page * taken;
                    var url = SetUrlParameters(request, new Dictionary<string, object> { { PageSkipParam, toSkip } });

                    paginationListBuilder.InnerHtml.AppendHtml(GeneratePageLink((page + 1).ToString(), url, isActive));
                }
            }

            var lastSkip = ((total / taken) - ((total % taken == 0) ? 1 : 0)) * taken;
            var lastUrl = SetUrlParameters(request, new Dictionary<string, object> { { PageSkipParam, lastSkip } });
            paginationListBuilder.InnerHtml.AppendHtml(GeneratePageLink(LastPageText, lastUrl));

            paginationNavBuilder.InnerHtml.AppendHtml(paginationListBuilder);

            paginationButtonsPanelBuilder.InnerHtml.AppendHtml(paginationNavBuilder);
            paginationPanelBuilder.InnerHtml.AppendHtml(paginationButtonsPanelBuilder);

            return paginationPanelBuilder;
        }

        private object GetElementAlignment(ElementAlignment alignment)
        {
            switch (alignment)
            {
                case ElementAlignment.Center:
                    return "justify-content-md-center";
                case ElementAlignment.Left:
                    return "justify-content-md-start ml-md-1";
                case ElementAlignment.Right:
                default:
                    return "justify-content-md-end mr-md-1";
            }
        }

        private IHtmlContent GeneratePageLink(string text, string url, bool active = false)
        {
            var linkListBuilder = new TagBuilder("li");
            linkListBuilder.AddCssClass("page-item");

            if (active)
                linkListBuilder.AddCssClass("active");

            var linkBuilder = new TagBuilder("a");
            linkBuilder.AddCssClass("page-link");
            linkBuilder.Attributes.Add("href", url);
            linkBuilder.InnerHtml.Append(text);

            linkListBuilder.InnerHtml.AppendHtml(linkBuilder);

            return linkListBuilder;
        }

        private string SetUrlParameters(HttpRequest request, IDictionary<string, object> parameters)
        {
            var qs = HttpUtility.ParseQueryString(request.QueryString.ToString());

            foreach (var param in parameters)
                qs.Set(param.Key, param.Value.ToString());

            var currentUrl = Microsoft.AspNetCore.Http.Extensions.UriHelper.GetDisplayUrl(request);
            var uriBuilder = new UriBuilder(currentUrl) { Query = qs.ToString() };

            return uriBuilder.Uri.ToString();
        }
    }
}
