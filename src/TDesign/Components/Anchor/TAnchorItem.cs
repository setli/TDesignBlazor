﻿using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace TDesign
{
    /// <summary>
    /// TAnchorItem 目标
    /// </summary>
    public enum AnchorItemTarget
    {
        /// <summary>
        /// 在当前页面加载
        /// </summary>
        [HtmlAttribute("_self")] Self,

        /// <summary>
        /// 在新窗口打开
        /// </summary>
        [HtmlAttribute("_blank")] Blank,

        /// <summary>
        /// 如果没有 parent 框架或者浏览上下文，此选项的行为方式与 _self 相同。
        /// 详见<see href="https://developer.mozilla.org/zh-CN/docs/Web/HTML/Element/a#attr-target"/>
        /// </summary>
        [HtmlAttribute("_parent")] Parent,

        /// <summary>
        /// 如果没有 parent 框架或者浏览上下文，此选项的行为方式相同_self。
        /// 详见<see href="https://developer.mozilla.org/zh-CN/docs/Web/HTML/Element/a#attr-target"/>
        /// </summary>
        [HtmlAttribute("_top")] Top
    }

    /// <summary>
    /// 锚点子级
    /// </summary>
    [HtmlTag("div")]
    [CssClass("t-anchor__item")]
    [ChildComponentAttribute(typeof(TAnchor))]
    public class TAnchorItem : BlazorComponentBase, IHasChildContent, IHasActive
    {
        private string? _href;



        /// <summary>
        /// 获取或设置选中状态
        /// </summary>
        [Parameter] public bool Active { get; set; }

        /// <summary>
        /// 用于自动化获取父组件。
        /// </summary>
        [CascadingParameter] public TAnchor? CascadingAnchor { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// 锚点
        /// </summary>
        [Parameter]
        public string? Href
        {
            get
            {
                return _href;
            }
            set
            {
                var anchors = NavigationManager?.Uri.Split('#') ?? Array.Empty<string>();
                if (anchors.Length > 1)
                {
                    _href = NavigationManager?.Uri?.Replace($"#{anchors[^1]}", value) ?? "";
                }
                else
                {
                    _href = NavigationManager?.Uri + value;
                }
            }
        }

        /// <summary>
        /// 获取或设置偏移的高度
        /// </summary>
        public int OffsetHeight { get; set; }

        /// <summary>
        /// 获取或设置顶部偏移
        /// </summary>
        public int OffsetTop { get; set; }

        /// <summary>
        /// 点击事件
        /// </summary>
        [Parameter][HtmlAttribute("onclick")] public EventCallback<MouseEventArgs?> OnClick { get; set; }

        /// <summary>
        /// 锚点文字
        /// </summary>
        [Parameter] public AnchorItemTarget? Target { get; set; } = AnchorItemTarget.Self;

        /// <summary>
        /// 标题
        /// </summary>
        [Parameter] public string? Title { get; set; }

        internal int Index { get; private set; }
        [Inject] private NavigationManager? NavigationManager { get; set; }

        /// <summary>
        /// 设置选中状态
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            Active = active;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="sequence"></param>
        protected override void AddContent(RenderTreeBuilder builder, int sequence)
        {
            builder.CreateComponent<TLink>(sequence + 1, Title,
                           attributes: new
                           {
                               Href,
                               Title,
                               Target = Target?.GetHtmlAttribute(),
                               onclick = HtmlHelper.Event.Create<MouseEventArgs>(this, async x =>
                               {
                                   CascadingAnchor.ClickLoad = true;
                                   for (int i = 0; i < CascadingAnchor?.ChildComponents.Count; i++)
                                   {
                                       if (CascadingAnchor.ChildComponents[i] is TAnchorItem item)
                                       {
                                           if (Index == i)
                                           {
                                               item.Active = true;
                                           }
                                           else
                                           {
                                               item.Active = false;
                                           }

                                           await item.Refresh();
                                       }
                                   }
                                   var anchorObj = await JS.Value.InvokeAsync<IJSObjectReference>("import", "./_content/TDesign/tdesign-blazor.js");
                                   await anchorObj.InvokeVoidAsync("anchor.hash", Href?.Split("#")[1], CascadingAnchor?.Container?.Split("#")[1]);

                                   await this.Refresh();
                                   CascadingAnchor!.SwitchIndex = Index;
                                   await CascadingAnchor.Refresh();
                                   CascadingAnchor.ClickLoad = false;
                               }),
                               @class = "t-anchor__item-link"
                           });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="builder"></param>
        protected override void BuildCssClass(ICssClassBuilder builder)
        {
            if (Active)
            {
                builder.Append("t-is-active");
            }
            else
            {
                builder.Remove("t-is-active");
            }
            base.BuildCssClass(builder);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            Index = CascadingAnchor!.ChildComponents.Count - 1;
        }
    }
}