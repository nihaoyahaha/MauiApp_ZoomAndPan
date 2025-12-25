namespace MauiApp_ZoomAndPan;

public class ZoomPanContainer : ContentView
{
	double totalScale = 1.0;
	double startScale = 1.0;
	double xOffset = 0;
	double yOffset = 0;
	Point? initialPinchOrigin = null;

	public ZoomPanContainer()
	{
		// 添加捏合手势
		var pinchGesture = new PinchGestureRecognizer();
		pinchGesture.PinchUpdated += OnPinchUpdated;
		GestureRecognizers.Add(pinchGesture);

		// 添加平移（拖拽）手势
		var panGesture = new PanGestureRecognizer();
		panGesture.PanUpdated += OnPanUpdated;
		GestureRecognizers.Add(panGesture);
	}

	/// <summary>
	/// 计算控件在父容器内的边界数据
	/// </summary>
	/// <param name="parent">父容器</param>
	/// <returns>父容器宽度，父容器高度，缩放后的内容宽度，缩放后的内容高度，x轴最小偏移值，x轴最大偏移值，y轴最小偏移值，y轴最大偏移值，内容宽度是否越界，内容高度是否越界</returns>
	(double parentWidth, double parentHeight, double scaledW, double scaledH, double xOffsetMin, double xOffsetMax, double yOffsetMin, double yOffsetMax, bool isWidthOutOfBounds, bool isHeightOutOfBounds) CalculateTranslationBounds(VisualElement parent)
	{
		double parentWidth = parent.Width;
		double parentHeight = parent.Height;

		double scaledW = Content.DesiredSize.Width * totalScale;
		double scaledH = Content.DesiredSize.Height * totalScale;

		bool isWidthOutOfBounds = scaledW > parentWidth;
		bool isHeightOutOfBounds = scaledH > parentHeight;

		double xOffsetMin;
		double xOffsetMax;
		double yOffsetMin;
		double yOffsetMax;

		if (isWidthOutOfBounds)
		{
			xOffsetMin = -((Content.DesiredSize.Width * totalScale - parentWidth) + (parentWidth - Content.DesiredSize.Width) / 2);
			xOffsetMax = -((parentWidth - Content.DesiredSize.Width) / 2);
		}
		else
		{
			xOffsetMin = -((parentWidth - Content.DesiredSize.Width) / 2);
			xOffsetMax = (parentWidth - scaledW) / 2;//0;
		}

		if (isHeightOutOfBounds)
		{
			yOffsetMin = -(Content.DesiredSize.Height * totalScale - parentHeight + (parentHeight - Content.DesiredSize.Height) / 2);
			yOffsetMax = -((parentHeight - Content.DesiredSize.Height) / 2);
		}
		else
		{
			yOffsetMin = -((parentHeight - Content.DesiredSize.Height) / 2);
			yOffsetMax = (parentHeight - scaledH) / 2;//0;
		}
		return (parentWidth, parentHeight, scaledW, scaledH, xOffsetMin, xOffsetMax, yOffsetMin, yOffsetMax, isWidthOutOfBounds, isHeightOutOfBounds);
	}

	//捏合手势
	void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
	{
		try
		{
			if (Content == null || Parent is not VisualElement parent) return;

			totalScale *= e.Scale;
			totalScale = Math.Max(0.25, Math.Min(8, totalScale));

			var bounds = CalculateTranslationBounds(parent);

			if (e.Status == GestureStatus.Started)
			{
				startScale = Content.Scale;
				Content.AnchorX = 0;
				Content.AnchorY = 0;

				Content.TranslationX = xOffset;
				Content.TranslationY = yOffset;
				initialPinchOrigin = e.ScaleOrigin;
			}
			else if (e.Status == GestureStatus.Running)
			{
				Content.Scale = totalScale;
				var origin = initialPinchOrigin.Value;

				double renderedX = Content.X + xOffset;
				double deltaX = renderedX / bounds.parentWidth;
				double deltaWidth = bounds.parentWidth / (Content.DesiredSize.Width * startScale);
				double originX = (origin.X - deltaX) * deltaWidth;

				double renderedY = Content.Y + yOffset;
				double deltaY = renderedY / bounds.parentHeight;
				double deltaHeight = bounds.parentHeight / (Content.DesiredSize.Height * startScale);
				double originY = (origin.Y - deltaY) * deltaHeight;

				double targetX = xOffset - (originX * Content.DesiredSize.Width) * (totalScale - startScale);
				double targetY = yOffset - (originY * Content.DesiredSize.Height) * (totalScale - startScale);

				if (bounds.isWidthOutOfBounds)
				{
					Content.TranslationX = Math.Clamp(targetX, bounds.xOffsetMin, bounds.xOffsetMax);
				}
				else
				{
					var offset1 = (bounds.parentWidth - bounds.scaledW) / 2;
					var offset2 = (bounds.parentWidth - Content.DesiredSize.Width) / 2;

					Content.TranslationX = Math.Clamp(-(offset2 - offset1), bounds.xOffsetMin, bounds.xOffsetMax);
				}

				if (bounds.isHeightOutOfBounds)
				{
					Content.TranslationY = Math.Clamp(targetY, bounds.yOffsetMin, bounds.yOffsetMax);
				}
				else
				{
					var offset1 = (bounds.parentHeight - bounds.scaledH) / 2;
					var offset2 = (bounds.parentHeight - Content.DesiredSize.Height) / 2;
					Content.TranslationY = Math.Clamp(-(offset2 - offset1), bounds.yOffsetMin, bounds.yOffsetMax);
				}

			}
			else if (e.Status == GestureStatus.Completed)
			{
				xOffset = Content.TranslationX;
				yOffset = Content.TranslationY;
				initialPinchOrigin = null;
			}
		}
		catch (Exception ex)
		{

		}
	}

	//平移手势
	void OnPanUpdated(object sender, PanUpdatedEventArgs e)
	{
		if (Content == null || Parent is not VisualElement parent) return;
		if (initialPinchOrigin != null) return;

		var bounds = CalculateTranslationBounds(parent);

		if (e.StatusType == GestureStatus.Running)
		{
			if (bounds.isWidthOutOfBounds)
			{
				Content.TranslationX = Math.Clamp(xOffset + e.TotalX, bounds.xOffsetMin, bounds.xOffsetMax);
			}
			if (bounds.isHeightOutOfBounds)
			{
				Content.TranslationY = Math.Clamp(yOffset + e.TotalY, bounds.yOffsetMin, bounds.yOffsetMax);
			}
		}
		else if (e.StatusType == GestureStatus.Completed)
		{
			xOffset = Content.TranslationX;
			yOffset = Content.TranslationY;
		}
	}



}