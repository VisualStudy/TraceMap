import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:trace_map/main.dart';

void main() {
  testWidgets('TraceMap app builds', (WidgetTester tester) async {
    await tester.pumpWidget(const MyApp());
    expect(find.byType(MaterialApp), findsOneWidget);
  });
}
