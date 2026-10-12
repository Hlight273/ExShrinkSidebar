## Why

配置树拖拽只有边框变色，缺少节点跟随反馈；修改边框厚度还可能影响布局。长列表自动滚动时，也需要持续重新计算落点，让插入提示与松手后的排序结果保持一致。

## What Changes

- 使用 WPF 原生拖放和 Adorner 展示一次性内存快照，按渲染帧跟随指针。
- 原位置淡化并保留占位，独立绘制插入线，滚动后重新命中目标。
- 输入框、按钮和下拉框不启动节点拖拽；取消、结束及关闭时清理视觉与事件。

## Capabilities

### Modified Capabilities
- `config-editor`: 拖拽视觉反馈和交互边界。

## Impact

修改 AddConfigPanel、SubConfigView，新增节点拖拽 Adorner。配置结构和同级排序约束不变。
