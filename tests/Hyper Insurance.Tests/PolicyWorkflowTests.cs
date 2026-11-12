using HyperInsurance.Controllers;
using HyperInsurance.Models;
using HyperInsurance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hyper_Insurance.Tests;

[TestClass]
public sealed class PolicyWorkflowTests : DatabaseTestBase
{
    [TestMethod]
    public async Task IssuingPolicySavesSelectedCoverageAndIncludesItsPremium()
    {
        var records = await CreateApplicationAsync();
        var coverage = await CreateCoverageAsync();
        var controller = CreatePoliciesController();

        var result = await controller.IssuePolicy(
            records.Application.Id,
            new IssuePolicyRequest
            {
                Coverages =
                [
                    new PolicyCoverageSelectionRequest
                    {
                        CoverageId = coverage.Id,
                        CoverageLimit = 50000m,
                        Deductible = 1000m
                    }
                ]
            },
            CancellationToken.None);

        Assert.AreEqual(201, GetStatusCode(result));
        var response = GetResponseValue<PolicyResponse>(result);
        Assert.AreEqual(2425m, response.AnnualPremium);
        Assert.AreEqual(1, response.Coverages.Count);
        Assert.AreEqual("Collision", response.Coverages[0].Name);
        Assert.AreEqual(175m, response.Coverages[0].AdditionalPremium);

        var savedPolicy = await Context.Policies.FindAsync(response.Id);
        Assert.IsNotNull(savedPolicy);
        Assert.AreEqual(1, await Context.PolicyCoverages.CountAsync());
    }

    [TestMethod]
    public async Task DuplicateCoverageSelectionsAreRejected()
    {
        var records = await CreateApplicationAsync();
        var coverage = await CreateCoverageAsync();
        var selection = new PolicyCoverageSelectionRequest
        {
            CoverageId = coverage.Id,
            CoverageLimit = 50000m,
            Deductible = 1000m
        };

        var result = await CreatePoliciesController().IssuePolicy(
            records.Application.Id,
            new IssuePolicyRequest { Coverages = [selection, selection] },
            CancellationToken.None);

        Assert.AreEqual(400, GetStatusCode(result));
        Assert.AreEqual(0, await Context.Policies.CountAsync());
    }

    [TestMethod]
    public async Task InvalidOrInactiveCoverageTermsAreRejected()
    {
        var records = await CreateApplicationAsync();
        var coverage = await CreateCoverageAsync();
        var controller = CreatePoliciesController();

        var invalidTermsResult = await controller.IssuePolicy(
            records.Application.Id,
            new IssuePolicyRequest
            {
                Coverages =
                [
                    new PolicyCoverageSelectionRequest
                    {
                        CoverageId = coverage.Id,
                        CoverageLimit = 100000m,
                        Deductible = 1000m
                    }
                ]
            },
            CancellationToken.None);

        Assert.AreEqual(400, GetStatusCode(invalidTermsResult));
        Assert.AreEqual(0, await Context.Policies.CountAsync());

        var inactiveCoverage = await CreateCoverageAsync(
            name: "Inactive",
            isActive: false);
        var inactiveResult = await controller.IssuePolicy(
            records.Application.Id,
            new IssuePolicyRequest
            {
                Coverages =
                [
                    new PolicyCoverageSelectionRequest
                    {
                        CoverageId = inactiveCoverage.Id,
                        CoverageLimit = 50000m,
                        Deductible = 1000m
                    }
                ]
            },
            CancellationToken.None);

        Assert.AreEqual(400, GetStatusCode(inactiveResult));
        Assert.AreEqual(0, await Context.Policies.CountAsync());
    }

    [TestMethod]
    public async Task PolicyCanBeRenewedAndCancelled()
    {
        var records = await CreateApplicationAsync();
        var coverage = await CreateCoverageAsync();
        var policiesController = CreatePoliciesController();
        var issued = await policiesController.IssuePolicy(
            records.Application.Id,
            new IssuePolicyRequest
            {
                Coverages =
                [
                    new PolicyCoverageSelectionRequest
                    {
                        CoverageId = coverage.Id,
                        CoverageLimit = 50000m,
                        Deductible = 1000m
                    }
                ]
            },
            CancellationToken.None);
        var policy = GetResponseValue<PolicyResponse>(issued);

        var renewed = await policiesController.RenewPolicy(
            policy.Id,
            CancellationToken.None);

        Assert.AreEqual(200, GetStatusCode(renewed));
        var renewedPolicy = GetResponseValue<PolicyResponse>(renewed);
        Assert.AreEqual(policy.ExpiryDate.AddYears(1), renewedPolicy.ExpiryDate);

        var cancelled = await policiesController.CancelPolicy(
            policy.Id,
            CancellationToken.None);

        Assert.AreEqual(200, GetStatusCode(cancelled));
        Assert.AreEqual(
            PolicyStatuses.Cancelled,
            GetResponseValue<PolicyOperationResponse>(cancelled).Status);

        var renewCancelled = await policiesController.RenewPolicy(
            policy.Id,
            CancellationToken.None);
        Assert.AreEqual(409, GetStatusCode(renewCancelled));
    }

    [TestMethod]
    public async Task AddingCoverageUpdatesPremiumAndRejectsDuplicateSelection()
    {
        var records = await CreateApplicationAsync();
        var firstCoverage = await CreateCoverageAsync();
        var secondCoverage = await CreateCoverageAsync(
            name: "Roadside",
            additionalPremium: 80m,
            deductibleAdjustment: 0m);
        var policiesController = CreatePoliciesController();
        var issued = await policiesController.IssuePolicy(
            records.Application.Id,
            new IssuePolicyRequest
            {
                Coverages =
                [
                    new PolicyCoverageSelectionRequest
                    {
                        CoverageId = firstCoverage.Id,
                        CoverageLimit = 50000m,
                        Deductible = 1000m
                    }
                ]
            },
            CancellationToken.None);
        var policy = GetResponseValue<PolicyResponse>(issued);
        var coveragesController = new PolicyCoveragesController(Context);
        ConfigureController(coveragesController);

        var added = await coveragesController.AddCoverage(
            policy.Id,
            secondCoverage.Id,
            new SelectCoverageTermsRequest
            {
                CoverageLimit = 50000m,
                Deductible = 1000m
            },
            CancellationToken.None);

        Assert.AreEqual(200, GetStatusCode(added));
        Assert.AreEqual(80m, GetResponseValue<PolicyCoverageResponse>(added).AdditionalPremium);
        Assert.AreEqual(2505m, (await Context.Policies.FindAsync(policy.Id))!.AnnualPremium);

        var duplicate = await coveragesController.AddCoverage(
            policy.Id,
            secondCoverage.Id,
            new SelectCoverageTermsRequest
            {
                CoverageLimit = 50000m,
                Deductible = 1000m
            },
            CancellationToken.None);
        Assert.AreEqual(409, GetStatusCode(duplicate));
    }

    private PoliciesController CreatePoliciesController()
    {
        var controller = new PoliciesController(Context, new RatingService());
        ConfigureController(controller);
        return controller;
    }
}
