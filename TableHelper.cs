using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace StageZero.TableHelper
{
    public static class TableHelper
    {
        /// <summary>
        /// Creates an HTML table from the items provided.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="items">Items to show in table</param>
        /// <param name="tableSetup">Setup of table</param>
        /// <returns></returns>
        public static IHtmlContent Table<TItem>(this IHtmlHelper htmlHelper, IQueryable<TItem> items, Action<Table<TItem>> tableSetup, object htmlAttributes = null)
        {
            return htmlHelper.Table(null, items, tableSetup, htmlAttributes);
        }

        /// <summary>
        /// Creates an HTML table from the items provided.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="id">Id unique to this table for this page. Must be URL compatible.</param>
        /// <param name="items">Items to show in table</param>
        /// <param name="tableSetup">Setup of table</param>
        /// <returns></returns>
        public static IHtmlContent Table<TItem>(this IHtmlHelper htmlHelper, string id, IQueryable<TItem> items, Action<Table<TItem>> tableSetup, object htmlAttributes = null)
        {
            var table = new Table<TItem>(htmlHelper, items, id, htmlAttributes);

            tableSetup(table);

            return table.Render();
        }
    }
}
