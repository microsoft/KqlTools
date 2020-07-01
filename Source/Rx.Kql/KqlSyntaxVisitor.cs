// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using Kusto.Language.Syntax;
    using System;

    abstract class KqlSyntaxVisitor<T> : SyntaxVisitor<T>
    {
        public override T VisitAdminCommand(AdminCommand node)
        {
            throw new NotImplementedException();
        }

        public override T VisitAliasStatement(AliasStatement node)
        {
            throw new NotImplementedException();
        }

        public override T VisitAsOperator(AsOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitAtExpression(AtExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitBadCommand(BadCommand node)
        {
            throw new NotImplementedException();
        }

        public override T VisitBadQueryOperator(BadQueryOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitBetweenExpression(BetweenExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitBinaryExpression(Kusto.Language.Syntax.BinaryExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitBrackettedExpression(BrackettedExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitBrackettedNameDeclaration(BrackettedNameDeclaration node)
        {
            throw new NotImplementedException();
        }

        public override T VisitBrackettedNameReference(BrackettedNameReference node)
        {
            throw new NotImplementedException();
        }

        public override T VisitClientParameterReference(ClientParameterReference node)
        {
            throw new NotImplementedException();
        }

        public override T VisitCommandInputExpression(CommandInputExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitCommandWithPropertyListClause(CommandWithPropertyListClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitCommandWithValueClause(CommandWithValueClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitCompoundNamedExpression(CompoundNamedExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitCompoundStringLiteralExpression(CompoundStringLiteralExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitConditionedExpression(ConditionedExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitConsumeOperator(ConsumeOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitCountAsIdentifierClause(CountAsIdentifierClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitCountOperator(CountOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitCreateFunctionCommand(CreateFunctionCommand node)
        {
            throw new NotImplementedException();
        }

        public override T VisitCustom(CustomNode node)
        {
            throw new NotImplementedException();
        }

        public override T VisitDataScopeClause(DataScopeClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitDataScopeExpression(DataScopeExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitDataTableExpression(DataTableExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitDefaultExpressionClause(DefaultExpressionClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitDefaultValueDeclaration(DefaultValueDeclaration node)
        {
            throw new NotImplementedException();
        }

        public override T VisitDirective(Directive node)
        {
            throw new NotImplementedException();
        }

        public override T VisitDistinctOperator(DistinctOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitDynamicExpression(DynamicExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitElementExpression(ElementExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitEvaluateOperator(Kusto.Language.Syntax.EvaluateOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitExecuteAndCacheOperator(ExecuteAndCacheOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitExpressionCouple(ExpressionCouple node)
        {
            throw new NotImplementedException();
        }

        public override T VisitExpressionList(ExpressionList node)
        {
            throw new NotImplementedException();
        }

        public override T VisitExpressionStatement(ExpressionStatement node)
        {
            throw new NotImplementedException();
        }

        public override T VisitExtendOperator(Kusto.Language.Syntax.ExtendOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitExternalDataExpression(ExternalDataExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitExternalDataWithClause(ExternalDataWithClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitFacetOperator(FacetOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitFacetWithExpressionClause(FacetWithExpressionClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitFacetWithOperatorClause(FacetWithOperatorClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitFilterOperator(FilterOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitFindInClause(FindInClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitFindOperator(FindOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitFindProjectClause(FindProjectClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitForkExpression(ForkExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitForkOperator(ForkOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitFunctionBody(FunctionBody node)
        {
            throw new NotImplementedException();
        }

        public override T VisitFunctionCallExpression(FunctionCallExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitFunctionDeclaration(FunctionDeclaration node)
        {
            throw new NotImplementedException();
        }

        public override T VisitFunctionParameter(FunctionParameter node)
        {
            throw new NotImplementedException();
        }

        public override T VisitFunctionParameters(FunctionParameters node)
        {
            throw new NotImplementedException();
        }

        public override T VisitGeneralCommand(GeneralCommand node)
        {
            throw new NotImplementedException();
        }

        public override T VisitGetSchemaOperator(GetSchemaOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitIdentifierNameDeclaration(IdentifierNameDeclaration node)
        {
            throw new NotImplementedException();
        }

        public override T VisitIdentifierNameReference(IdentifierNameReference node)
        {
            throw new NotImplementedException();
        }

        public override T VisitInExpression(InExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitInvokeOperator(InvokeOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitJoinOnClause(JoinOnClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitJoinOnSegmentsClause(JoinOnSegmentsClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitJoinOperator(JoinOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitJoinWhereClause(JoinWhereClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitJoinWithWildcardsClause(JoinWithWildcardsClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitJoinWithWildcardsExpression(JoinWithWildcardsExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitJsonArrayExpression(JsonArrayExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitJsonObjectExpression(JsonObjectExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitJsonPair(JsonPair node)
        {
            throw new NotImplementedException();
        }

        public override T VisitLetStatement(LetStatement node)
        {
            throw new NotImplementedException();
        }

        public override T VisitList(SyntaxList list)
        {
            throw new NotImplementedException();
        }

        public override T VisitLiteralExpression(LiteralExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitLookupOperator(LookupOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitMakeSeriesByClause(MakeSeriesByClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitMakeSeriesExpression(MakeSeriesExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitMakeSeriesFromToStepClause(MakeSeriesFromToStepClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitMakeSeriesInRangeClause(MakeSeriesInRangeClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitMakeSeriesOnClause(MakeSeriesOnClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitMakeSeriesOperator(MakeSeriesOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitMaterializeExpression(MaterializeExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitMvApplyContextIdClause(MvApplyContextIdClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitMvApplyExpression(MvApplyExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitMvApplyOperator(MvApplyOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitMvApplyRowLimitClause(MvApplyRowLimitClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitMvApplySubqueryExpression(MvApplySubqueryExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitMvExpandExpression(MvExpandExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitMvExpandOperator(MvExpandOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitMvExpandRowLimitClause(MvExpandRowLimitClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitNameAndTypeDeclaration(NameAndTypeDeclaration node)
        {
            throw new NotImplementedException();
        }

        public override T VisitNamedParameter(NamedParameter node)
        {
            throw new NotImplementedException();
        }

        public override T VisitNameEqualsClause(NameEqualsClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitOptionValueClause(OptionValueClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitOrderedExpression(OrderedExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitOrderingClause(OrderingClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitOrderingNullsClause(OrderingNullsClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitPackExpression(PackExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitParenthesizedExpression(ParenthesizedExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitParseOperator(ParseOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitPartitionExpression(PartitionExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitPartitionOperator(PartitionOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitPathExpression(PathExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitPatternDeclaration(PatternDeclaration node)
        {
            throw new NotImplementedException();
        }

        public override T VisitPatternMatch(PatternMatch node)
        {
            throw new NotImplementedException();
        }

        public override T VisitPatternPathParameter(PatternPathParameter node)
        {
            throw new NotImplementedException();
        }

        public override T VisitPatternPathValue(PatternPathValue node)
        {
            throw new NotImplementedException();
        }

        public override T VisitPatternStatement(PatternStatement node)
        {
            throw new NotImplementedException();
        }

        public override T VisitPipeExpression(PipeExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitPrefixUnaryExpression(PrefixUnaryExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitPrimitiveTypeExpression(PrimitiveTypeExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitPrintOperator(PrintOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitProjectAwayOperator(ProjectAwayOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitProjectOperator(Kusto.Language.Syntax.ProjectOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitProjectRenameOperator(ProjectRenameOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitQuery(Kusto.Language.Syntax.Query node)
        {
            throw new NotImplementedException();
        }

        public override T VisitQueryParametersStatement(QueryParametersStatement node)
        {
            throw new NotImplementedException();
        }

        public override T VisitRangeOperator(RangeOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitReduceByOperator(ReduceByOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitReduceByWithClause(ReduceByWithClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitRenameList(RenameList node)
        {
            throw new NotImplementedException();
        }

        public override T VisitRenderNameList(RenderNameList node)
        {
            throw new NotImplementedException();
        }

        public override T VisitRenderOperator(RenderOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitRenderWithClause(RenderWithClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitRestrictStatement(RestrictStatement node)
        {
            throw new NotImplementedException();
        }

        public override T VisitSampleDistinctOperator(SampleDistinctOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitSampleOperator(SampleOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitSchemaTypeExpression(SchemaTypeExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitSearchOperator(SearchOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitSeparatedElement(SeparatedElement separatedElement)
        {
            throw new NotImplementedException();
        }

        public override T VisitSerializeOperator(SerializeOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitSetOptionStatement(SetOptionStatement node)
        {
            throw new NotImplementedException();
        }

        public override T VisitSimpleNamedExpression(SimpleNamedExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitSkippedTokens(SkippedTokens node)
        {
            throw new NotImplementedException();
        }

        public override T VisitSortOperator(SortOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitStarExpression(StarExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitSummarizeByClause(SummarizeByClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitSummarizeOperator(Kusto.Language.Syntax.SummarizeOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitTableSetCommand(TableSetCommand node)
        {
            throw new NotImplementedException();
        }

        public override T VisitTakeOperator(TakeOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitTopHittersByClause(TopHittersByClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitTopHittersOperator(TopHittersOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitTopNestedClause(TopNestedClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitTopNestedOperator(TopNestedOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitTopNestedWithOthersClause(TopNestedWithOthersClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitTopOperator(TopOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitToScalarExpression(ToScalarExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitToTableExpression(ToTableExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitToTypeOfClause(ToTypeOfClause node)
        {
            throw new NotImplementedException();
        }

        public override T VisitTypedColumnReference(TypedColumnReference node)
        {
            throw new NotImplementedException();
        }

        public override T VisitTypeOfLiteralExpression(TypeOfLiteralExpression node)
        {
            throw new NotImplementedException();
        }

        public override T VisitUnionOperator(UnionOperator node)
        {
            throw new NotImplementedException();
        }

        public override T VisitWildcardedNameReference(WildcardedNameReference node)
        {
            throw new NotImplementedException();
        }
    }
}
