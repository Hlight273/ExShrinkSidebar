## 实现与验证

- [x] 1. 删除延迟、动画、目标副本，保留即时描边和无整树刷新的排序。
- [x] 2. 增加名称后同级序号，并在初始化、增删、排序和语言刷新时更新。
- [x] 3. 补充中英文资源，通过 ResGen 重新生成访问器。
- [x] 4. 完整 MSBuild Debug 构建通过，输出到 bin/Debug/drag-preview-fix/。
- [x] 5. 同步规格、文档并通过严格校验及差异检查。
- [ ] 6. 人工验证即时拖拽、嵌套序号、增删排序和语言切换。

未使用 computer-use，未启动应用或创建测试产物。生成器输出统一为 UTF-8 无 BOM，并保留既有 CRLF 格式。

规格严格校验和本轮差异检查（兼容 CRLF）通过。静态检索确认拖拽代码中已无 PreviewDelay、ApplyPreview 或 SetPlacementPreview。运行时效果仍待人工验收。
