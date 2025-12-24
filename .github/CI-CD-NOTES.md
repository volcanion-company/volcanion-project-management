# CI/CD Pipeline - Configuration Notes

## ✅ Fixed Issues (December 24, 2025)

### 1. Docker Image Tag Mismatch ✅ FIXED
**Problem**: Security scan was using `${{ github.sha }}` tag, but the actual image tags from metadata-action could be different (branch names, PR numbers, semver, etc.)

**Solution**: Changed to use `${{ github.ref_name }}` which matches the branch-based tag that will always be created.

```yaml
# Before: image-ref: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.sha }}
# After:  image-ref: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.ref_name }}
```

### 2. Notification Job Dependencies ✅ FIXED
**Problem**: Notification job had `needs: [build-and-test, docker-build, deploy-staging, deploy-production]`, but deploy jobs are conditional and may be skipped, causing the notification job to also be skipped.

**Solution**: Removed deployment jobs from dependencies. Notification now only depends on core jobs that always run.

```yaml
# Before: needs: [build-and-test, docker-build, deploy-staging, deploy-production]
# After:  needs: [build-and-test, docker-build]
```

### 3. Deployment Image References ✅ FIXED
**Problem**: Deployment steps referenced images by SHA, but branch-based tags are more reliable for deployment references.

**Solution**: Updated deployment placeholders to use both `ref_name` and `sha` for better tracking.

```yaml
echo "Image: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.ref_name }}"
echo "SHA: ${{ github.sha }}"
```

## ⚠️ Known Limitations

### 1. .NET 10.0 Preview
**Issue**: Project uses .NET 10.0 which is currently in preview (as of Dec 2024).

**Impact**: 
- GitHub Actions runners should support it via `actions/setup-dotnet@v4`
- May require `include-prerelease: true` if SDK is not yet released
- Build times may be slightly longer for preview versions

**Recommendation**: Monitor .NET 10 release schedule and update when RTM is available.

### 2. Health Check in Docker
**Issue**: Docker health check in compose file uses `curl` but the runtime image doesn't include it by default.

**Current Status**: Container shows as "unhealthy" in `docker ps`, but API is actually working fine.

**Solution Options**:
1. Install `curl` in Dockerfile: `RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*`
2. Use .NET health check instead: Use a custom health check script with `dotnet`
3. Accept the status (API works regardless)

**Recommendation**: Add curl to Dockerfile for production deployments.

### 3. Missing Actual Deployment Steps
**Issue**: Deploy jobs have placeholder steps only.

**Required for Production**:
- Kubernetes/Azure/AWS deployment commands
- Database migration strategy
- Health check verification
- Rollback procedures
- Blue-green or canary deployment strategy

## 📋 Pre-Deployment Checklist

Before enabling this CI/CD pipeline for production:

### GitHub Repository Settings
- [ ] Set up GitHub Secrets:
  - [ ] `EMAIL_USERNAME` - for password reset emails (optional for initial deployment)
  - [ ] `EMAIL_PASSWORD` - Gmail app-specific password (optional)
  - [ ] `KUBE_CONFIG_STAGING` - Kubernetes config for staging (if using K8s)
  - [ ] `KUBE_CONFIG_PRODUCTION` - Kubernetes config for production (if using K8s)
  - [ ] `SONAR_TOKEN` - If using SonarCloud (optional)
  - [ ] Deployment-specific secrets (Azure credentials, AWS keys, etc.)

- [ ] Enable GitHub Container Registry (ghcr.io):
  - Settings → Packages → Connect repository
  - Set package visibility (private/public)

- [ ] Configure Branch Protection Rules:
  - Require pull request reviews for `main` branch
  - Require status checks (all CI jobs must pass)
  - Require branches to be up to date

### Environment Configuration
- [ ] Create GitHub Environments:
  - `staging` environment with URL: https://staging.volcanionpm.com
  - `production` environment with URL: https://volcanionpm.com
  - Configure required reviewers for production

- [ ] Set up deployment targets:
  - [ ] Kubernetes cluster (staging + production)
  - [ ] Database instances
  - [ ] Redis instances
  - [ ] Load balancers/Ingress controllers
  - [ ] SSL certificates (Let's Encrypt or purchased)
  - [ ] DNS records

### Monitoring & Alerts
- [ ] Set up monitoring:
  - [ ] Application Insights / Prometheus
  - [ ] Error tracking (Sentry, Raygun, etc.)
  - [ ] Log aggregation (ELK, CloudWatch, etc.)

- [ ] Configure alerts:
  - [ ] Deployment failures
  - [ ] Health check failures
  - [ ] High error rates
  - [ ] Performance degradation

### Testing
- [ ] Test CI/CD pipeline on feature branch first
- [ ] Verify all 286 tests pass in GitHub Actions
- [ ] Test Docker image build completes
- [ ] Verify security scans work and upload SARIF
- [ ] Test staging deployment (when implemented)

## 🚀 Trigger Events

The pipeline is configured to run on:

1. **Push to main/develop branches**: Full pipeline including deployment
2. **Pull requests**: Build, test, and security scan only (no deployment)
3. **Manual dispatch**: Can be triggered manually via GitHub Actions UI

## 📊 Pipeline Flow

```
Push/PR → Build & Test (286 tests)
       ↓
    Code Analysis (SonarCloud placeholder)
       ↓
    Security Scan (Trivy filesystem)
       ↓
    Docker Build → Push to ghcr.io → Image Scan
       ↓
    Deploy (conditional)
       ├─ Staging (develop branch)
       └─ Production (main branch)
       ↓
    Notifications
```

## 🔒 Security Features

1. **Trivy Vulnerability Scanning**:
   - Filesystem scan before Docker build
   - Docker image scan after build
   - Results uploaded to GitHub Security tab

2. **Dependency Scanning**:
   - GitHub Dependabot enabled (recommended)
   - NuGet package vulnerability checks

3. **Container Security**:
   - Non-root user (uid 1001)
   - Minimal runtime image (aspnet:10.0)
   - No sensitive data in logs

4. **Secrets Management**:
   - All secrets via GitHub Secrets
   - No hardcoded credentials
   - Environment-specific configuration

## 📝 Maintenance Tasks

### Weekly
- Review security scan results in GitHub Security tab
- Check dependency updates (Dependabot PRs)
- Monitor build times and optimize if needed

### Monthly
- Review and update GitHub Actions versions (actions/checkout, etc.)
- Check for .NET SDK updates
- Review Docker base image updates

### Quarterly
- Review and update deployment strategies
- Audit access to GitHub Secrets
- Review monitoring and alert effectiveness

## 🆘 Troubleshooting

### Build Fails with "SDK not found"
**Solution**: .NET 10 SDK may not be available yet. Add `include-prerelease: true` to setup-dotnet step.

### Tests Fail in CI but Pass Locally
**Solution**: Check environment differences (timezone, culture, file paths). Use `[InlineData]` with absolute paths or environment variables.

### Docker Build Fails
**Solution**: 
- Check Dockerfile path: `src/VolcanionPM.API/Dockerfile`
- Verify .dockerignore is not excluding necessary files
- Check multi-stage build context

### Deployment Job Skipped
**Solution**: Deploy jobs only run on push (not PR) and require specific branches (develop/main).

### Security Scan Fails
**Solution**: Check SARIF file permissions and GitHub Advanced Security is enabled for the repository.

## 📚 References

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Docker Build Push Action](https://github.com/docker/build-push-action)
- [Trivy Security Scanner](https://github.com/aquasecurity/trivy-action)
- [.NET SDK Installation](https://docs.microsoft.com/en-us/dotnet/core/install/)
