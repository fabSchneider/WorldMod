using System;
using System.Collections.Generic;
using Fab.WorldMod.UI;
using UnityEngine.UIElements;

namespace Fab.WorldMod
{
	public class Modal : VisualElement
	{
		public static readonly string classname = "modal";
		public static readonly string containerClassname = classname + "__container";
		public static readonly string headerClassname = classname + "__header";
		public static readonly string titleClassname = classname + "__title";
		public static readonly string closeBtnClassname = classname + "__close-button";
		public static readonly string closeBtnHiddenClassname = closeBtnClassname + "--hidden";
		public static readonly string contentContainerClassname = classname + "__content";
		public static readonly string footerClassname = classname + "__footer";
		public static readonly string footerButtonClassname = footerClassname + "-button";

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			UxmlBoolAttributeDescription visibleCloseBtnAttr = new UxmlBoolAttributeDescription()
			{
				name = "visible-close-button",
				defaultValue = true
			};

			UxmlStringAttributeDescription titleAttr = new UxmlStringAttributeDescription()
			{
				name = "title",
				defaultValue = string.Empty
			};
			
			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				Modal modal = ve as Modal;

				modal.Title = titleAttr.GetValueFromBag(bag, cc);
				modal.VisibleCloseButton = visibleCloseBtnAttr.GetValueFromBag(bag, cc);
			}

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}
		}
		public new class UxmlFactory : UxmlFactory<Modal, UxmlTraits> { }


		public bool VisibleCloseButton
		{
			get => !headerCloseBtn.ClassListContains(closeBtnHiddenClassname);
			set => headerCloseBtn.EnableInClassList(closeBtnHiddenClassname, !value);
		}

		private VisualElement header;
		private Button headerCloseBtn;
		private Label titleLabel;
		private VisualElement content;
		private VisualElement footer;

		public override VisualElement contentContainer => content;

		public string Title
		{
			get => titleLabel.text;
			set => titleLabel.text = value;
		}

		public Label TitleLabel => titleLabel;

		public Modal()
		{
			AddToClassList(classname);
			var container = new VisualElement().WithClass(containerClassname);
			header = new VisualElement().WithClass(headerClassname);
			titleLabel = new Label().WithClass(titleClassname);
			headerCloseBtn = new Button(() => RemoveFromHierarchy()).WithClass(closeBtnClassname);
			headerCloseBtn.text = "";
			header.Add(titleLabel);
			header.Add(headerCloseBtn);
			content = new VisualElement().WithClass(contentContainerClassname);
			footer = new VisualElement().WithClass(footerClassname);

			container.Add(header);
			container.Add(content);
			container.Add(footer);

			hierarchy.Add(container);
		}

		public Button AddButton(string text, Action clickEvent)
		{
			Button btn = new Button(clickEvent).WithClass(footerButtonClassname);
			btn.text = text;
			footer.Add(btn);
			return btn;
		}
	}
}
