# **AI Agent Instruction Prompt: Clone and Enhance YoutubeDownloader**

Here's a comprehensive instruction prompt incorporating Cognitive Solver Architecture principles:  
---

MASTER INSTRUCTION: YoutubeDownloader Enhancement Project

Mission Statement  
Clone and create an enhanced, production-ready version of YoutubeDownloader from [https://github.com/Tyrrrz/YoutubeDownloader](https://github.com/Tyrrrz/YoutubeDownloader) that surpasses the original in functionality, user experience, and code quality while using only free/open-source tools. Request user approval before implementing any paid solutions.  
---

COGNITIVE SOLVER ARCHITECTURE FRAMEWORK  
PHASE 1: UNDERSTAND (Deep Analysis)  
Task: Before writing any code, thoroughly understand the original application.  
Required Actions:

1. Clone the repository and analyze its complete structure  
2. Study the comprehensive analysis provided below (see REFERENCE ANALYSIS section)  
3. Map all existing features, workflows, and user journeys  
4. Identify architectural patterns (MVVM, DI, async/await)  
5. Document all external dependencies and their purposes  
6. Understand the build pipeline and deployment process  
7. Analyze user pain points and improvement opportunities

Verification Checkpoint 1:

*  Can you explain the complete data flow from URL input to downloaded video?  
*  Can you describe all user workflows (single video, playlist, channel, search)?  
*  Have you identified the role of each major library (Avalonia, YoutubeExplode, FFmpeg)?  
*  Do you understand why certain architectural decisions were made?

Output: Create a structured understanding document covering architecture, workflows, dependencies, and improvement opportunities.  
---

PHASE 2: ANALYZE (Problem Decomposition)  
Task: Break down the enhancement project into manageable, prioritized components.  
Component Breakdown:

1. Core Infrastructure:  
   * Technology stack selection (must use free tools)  
   * Project structure and architecture  
   * Build and deployment pipeline  
   * Dependency management  
2. UI/UX Enhancements:  
   * Unified queue management system  
   * Drag-and-drop support  
   * Enhanced visual feedback (toasts, notifications)  
   * Download pause/resume functionality  
   * Batch operations interface  
   * Settings profiles/presets  
3. Feature Additions:  
   * Download scheduling  
   * Bandwidth limiting  
   * Advanced filtering (date range, duration, quality)  
   * History management with search  
   * Format profiles (music-only, best quality, mobile-friendly)  
   * Browser extension integration potential  
4. Performance Optimizations:  
   * Metadata caching system  
   * FFmpeg management improvements  
   * Chunked download with pause/resume  
   * Parallel download optimization  
   * Auto-retry with exponential backoff  
5. Error Handling & Resilience:  
   * Centralized exception management  
   * User-friendly error messages with remediation steps  
   * Automatic retry policies  
   * Network failure recovery  
   * Validation and sanitization  
6. Code Quality:  
   * Unit test coverage  
   * Integration tests  
   * Code documentation  
   * Performance profiling  
   * Security audit

Prioritization Matrix:

* P0 (Critical): Core download functionality, basic UI, error handling  
* P1 (High): Pause/resume, queue management, enhanced feedback  
* P2 (Medium): Advanced features, profiles, optimizations  
* P3 (Nice-to-have): Browser extension, cloud integration, analytics

Verification Checkpoint 2:

*  Is each component clearly defined with specific deliverables?  
*  Are dependencies between components identified?  
*  Is the prioritization justified and practical?  
*  Have you estimated complexity and development time for each component?

Output: Detailed component breakdown with priorities, dependencies, and implementation sequence.  
---

PHASE 3: SOLVE (Iterative Implementation)  
Task: Implement enhancements using a structured, iterative approach with continuous verification.  
Implementation Protocol:  
Step 3.1: Technology Stack Selection  
Requirement: Use only free/open-source tools unless user approves paid alternatives.  
Evaluation Criteria:

* Cross-platform compatibility (Windows, Linux, macOS)  
* Active community and maintenance  
* Performance characteristics  
* Learning curve and documentation  
* Long-term viability

Proposed Stack (all free):

* Framework: .NET 9.0 (free, cross-platform)  
* UI Framework: Avalonia UI 11.x (MIT license, free)  
* Design System: Material.Avalonia or FluentAvalonia (free)  
* MVVM: CommunityToolkit.Mvvm (free)  
* YouTube Interaction: YoutubeExplode (free, LGPL)  
* Media Processing: FFmpeg (free, GPL)  
* Database (for caching): SQLite or LiteDB (free)  
* Testing: xUnit, NUnit, or MSTest (free)  
* CI/CD: GitHub Actions (free for public repos)

Alternative Considerations Requiring Approval:

* If commercial UI component libraries offer significant advantages → ASK USER  
* If paid API services provide better reliability → ASK USER  
* If premium hosting/CDN needed for distribution → ASK USER

Verification Loop 3.1:  
FOR EACH technology choice:  
 1\. Verify it meets all requirements (free, cross-platform, maintained)  
 2\. Check license compatibility  
 3\. Validate against project goals  
 4\. Test basic functionality in prototype  
 5\. Document decision rationale  
---

Step 3.2: Project Setup & Architecture  
Actions:

1. Initialize new repository with proper .gitignore  
2. Set up solution structure:  
3. EnhancedYoutubeDownloader/  
4. ├── src/  
5. │   ├── Core/                    \# Business logic library  
6. │   ├── Desktop/                 \# Avalonia desktop app  
7. │   ├── Shared/                  \# Shared models/interfaces  
8. │   └── Tests/                   \# Test projects  
9. ├── docs/                        \# Documentation  
10. ├── scripts/                     \# Build scripts  
11. └── .github/workflows/           \# CI/CD  
12. Configure build pipeline with MSBuild/SDK-style projects  
13. Set up dependency injection container  
14. Implement logging infrastructure  
15. Create base MVVM infrastructure

Verification Loop 3.2:  
VERIFY:  
 ✓ Solution builds without errors  
 ✓ Projects reference each other correctly  
 ✓ DI container resolves test services  
 ✓ Logging writes to output  
 ✓ MVVM base classes function correctly  
---

Step 3.3: Core Feature Implementation (P0)  
For Each Feature:

1. Design Phase:  
   * Create interface contracts  
   * Define data models  
   * Plan async/await patterns  
   * Design error handling strategy  
2. Implementation Phase:  
   * Write failing tests first (TDD approach)  
   * Implement feature  
   * Handle edge cases  
   * Add logging and telemetry points  
3. Verification Phase (Critical):  
4. CHECK assumptions:  
5.  \- Are all external dependencies available?  
6.  \- Does the feature work offline/degraded?  
7.  \- Are rate limits considered?  
8. VERIFY calculations/logic:  
9.  \- Unit test coverage \> 80%  
10.  \- Integration tests pass  
11.  \- No memory leaks (profiler check)  
12. TEST edge cases:  
13.  \- Invalid URLs  
14.  \- Network failures  
15.  \- Corrupted downloads  
16.  \- Concurrent operations  
17.  \- Large playlists (1000+ videos)  
18. CONSIDER alternatives:  
19.  \- Is there a more efficient algorithm?  
20.  \- Can we reduce dependencies?  
21.  \- How does this scale?  
22. Error Correction:  
    * Address all test failures  
    * Fix identified bugs  
    * Refactor for clarity  
    * Update documentation  
23. Integration:  
    * Merge into main codebase  
    * Run full test suite  
    * Performance benchmark  
    * Update changelog

Priority Order:

1. URL parsing and validation  
2. YouTube metadata extraction (via YoutubeExplode)  
3. Quality/format selection  
4. Basic download engine  
5. Progress reporting  
6. File output and naming  
7. Basic UI (queue view)  
8. Settings persistence

Verification Loop 3.3 (Per Feature):  
BEFORE moving to next feature:  
 ✓ All tests pass  
 ✓ Code reviewed (self-review checklist)  
 ✓ No TODO/HACK comments remain  
 ✓ Documentation updated  
 ✓ Performance acceptable  
 ✓ Error handling comprehensive  
---

Step 3.4: Enhanced Features Implementation (P1-P2)  
Implement in order:

1. Pause/Resume System:  
   * Chunked download implementation  
   * State persistence  
   * Resume validation  
   * UI controls  
2. Unified Queue Management:  
   * Observable collection with filtering  
   * Drag-and-drop reordering  
   * Multi-select operations  
   * Status filtering (queued/active/complete/failed)  
3. Enhanced Feedback System:  
   * Toast notification service  
   * Rich error dialogs with actions  
   * Loading states with skeletons  
   * Completion sounds/notifications  
4. Metadata Caching:  
   * SQLite/LiteDB database  
   * Cache invalidation strategy  
   * Async cache operations  
   * Memory management  
5. Advanced Filtering:  
   * Date range picker  
   * Duration slider  
   * Quality presets  
   * Regex pattern matching  
6. Format Profiles:  
   * Profile data models  
   * Profile management UI  
   * Import/export functionality  
   * Per-channel profile assignment

For EACH enhancement:  
IMPLEMENT with verification loop:  
 1\. Design & plan  
 2\. Write tests  
 3\. Implement  
 4\. Verify (see 3.3 checklist)  
 5\. Integrate  
 6\. Document  
---

PHASE 4: VERIFY (Comprehensive Testing)  
Task: Systematic verification before considering the project complete.  
Verification Matrix:  
4.1 Functional Testing  
TEST each user workflow:  
 ✓ Single video download (various qualities)  
 ✓ Playlist download (small, medium, large)  
 ✓ Channel download  
 ✓ Search and download  
 ✓ Subtitle embedding  
 ✓ Tag injection  
 ✓ Pause and resume  
 ✓ Batch operations  
 ✓ Settings profiles  
 ✓ Authentication flow  
4.2 Error Handling Testing  
SIMULATE failure conditions:  
 ✓ Network disconnection mid-download  
 ✓ Invalid URLs  
 ✓ Private/deleted videos  
 ✓ Age-restricted content  
 ✓ Disk space exhaustion  
 ✓ FFmpeg crashes  
 ✓ Corrupted metadata  
 ✓ Rate limiting  
 ✓ Concurrent download limits  
4.3 Performance Testing  
BENCHMARK and validate:  
 ✓ Download speed matches network capacity  
 ✓ UI remains responsive under load  
 ✓ Memory usage stays bounded  
 ✓ Startup time \< 3 seconds  
 ✓ Settings load instantly  
 ✓ Large playlist handling (\>1000 items)  
4.4 Cross-Platform Testing  
VERIFY on each platform:  
 ✓ Windows 10/11 (x64, ARM64)  
 ✓ macOS (Intel, Apple Silicon)  
 ✓ Linux (Ubuntu, Fedora, Arch)  
 CHECK platform-specific:  
 ✓ File path handling  
 ✓ Native dialogs work  
 ✓ System tray integration  
 ✓ Update mechanism  
4.5 Security Testing  
AUDIT for vulnerabilities:  
 ✓ No credentials stored in plaintext  
 ✓ Path traversal prevention  
 ✓ Input sanitization  
 ✓ Safe deserialization  
 ✓ No arbitrary code execution  
 ✓ Dependencies have no known CVEs  
4.6 User Experience Testing  
VALIDATE usability:  
 ✓ First-time user can download within 1 minute  
 ✓ Error messages are actionable  
 ✓ UI is intuitive (no manual needed)  
 ✓ Keyboard shortcuts work  
 ✓ Accessibility features present  
 ✓ Dark/light themes consistent  
Verification Checkpoint 4:

*  All automated tests pass  
*  Manual testing completed across platforms  
*  Performance benchmarks meet targets  
*  Security audit shows no critical issues  
*  User acceptance criteria satisfied

---

PHASE 5: REFINE (Iterative Improvement)  
Task: Continuously improve based on verification results and meta-analysis.  
5.1 Meta-Recursive Self-Improvement Protocol  
After Each Major Cycle:  
EXAMINE own processes:  
 1\. What worked well in this development cycle?  
 2\. What caused delays or rework?  
 3\. Were estimates accurate?  
 4\. Did the architecture support changes easily?  
LEARN from experience:  
 1\. What errors were made repeatedly?  
 2\. Which verification steps caught the most issues?  
 3\. What assumptions were incorrect?  
 4\. How can the development process improve?  
PROGRESSIVELY enhance capabilities:  
 1\. Update internal checklists based on lessons  
 2\. Add new verification steps for discovered issues  
 3\. Refine estimation models  
 4\. Improve code templates and patterns  
5.2 Code Quality Refinement  
FOR EACH module:  
 ✓ Run static analysis (Roslyn analyzers)  
 ✓ Check code coverage (aim for \>80%)  
 ✓ Review complexity metrics (cyclomatic complexity)  
 ✓ Eliminate code smells  
 ✓ Ensure consistent style  
 ✓ Add XML documentation  
 ✓ Update architecture diagrams  
5.3 Performance Refinement  
PROFILE and optimize:  
 1\. Identify bottlenecks (dotTrace, PerfView)  
 2\. Optimize hot paths  
 3\. Reduce allocations  
 4\. Improve async patterns  
 5\. Benchmark improvements  
 6\. Document optimizations  
5.4 User Feedback Integration  
IF gathering user feedback:  
 1\. Set up anonymous telemetry (opt-in)  
 2\. Create feedback channels  
 3\. Prioritize reported issues  
 4\. Implement requested features  
 5\. Iterate on UX pain points  
---

META-CONSTRAINTS AND GUIDELINES  
Cost Control Protocol  
BEFORE implementing any solution that costs money:  
 1\. Research free alternatives thoroughly  
 2\. Prototype with free tier if available  
 3\. Document cost vs. benefit analysis  
 4\. Present options to user:  
    \- Free alternative with limitations  
    \- Paid solution with benefits  
    \- Hybrid approach  
 5\. WAIT for user approval  
 6\. Only proceed after explicit permission  
Dependency Management Rules  
BEFORE adding any new dependency:  
 1\. Check if existing libraries can solve the problem  
 2\. Evaluate:  
    ✓ License compatibility  
    ✓ Maintenance status (last update)  
    ✓ GitHub stars/downloads (popularity)  
    ✓ Known vulnerabilities  
    ✓ Bundle size impact  
    ✓ Cross-platform support  
 3\. Consider alternatives (minimum 3\)  
 4\. Document decision in ADR (Architecture Decision Record)  
 5\. Add to dependency audit log  
Quality Gates (Cannot Proceed Without)  
GATE 1 \- Before committing code:  
 ✓ Code compiles without warnings  
 ✓ All tests pass  
 ✓ Static analysis clean  
 ✓ Self-reviewed against checklist  
GATE 2 \- Before merging feature:  
 ✓ Integration tests pass  
 ✓ Code coverage maintained/improved  
 ✓ Documentation updated  
 ✓ No performance regressions  
GATE 3 \- Before release:  
 ✓ Full test suite passes  
 ✓ Cross-platform testing complete  
 ✓ Security audit clean  
 ✓ Performance benchmarks met  
 ✓ User documentation complete  
 ✓ Changelog updated  
---

REFERENCE ANALYSIS (From Previous Analysis)  
Original App Strengths to Preserve:

* Clean MVVM architecture  
* Cross-platform Avalonia UI  
* Separation of Core/UI concerns  
* No API key requirements  
* Modern C\# features  
* Material Design aesthetic

Critical Improvements to Implement:

1. Unified queue management \- Single pane for all download states  
2. Pause/resume functionality \- State persistence and recovery  
3. Enhanced error handling \- User-friendly messages with actions  
4. Metadata caching \- SQLite-based cache with invalidation  
5. Drag-and-drop support \- URLs anywhere in UI  
6. Batch operations \- Multi-select and apply settings  
7. Download scheduling \- Time-based queue execution  
8. Format profiles \- Quick presets for common scenarios  
9. Better feedback \- Toast notifications and loading states  
10. Performance optimization \- Parallel downloads, chunking, retry logic

Technical Specifications to Match or Exceed:

* .NET 9.0 \- Latest framework  
* Avalonia 11.x \- Cross-platform UI  
* YoutubeExplode \- Core YouTube interaction  
* FFmpeg 7.x \- Media processing  
* SQLite/LiteDB \- Caching layer (new)  
* xUnit \- Testing framework (new)

---

DELIVERABLES CHECKLIST  
Code Deliverables:

*  Complete source code with clear structure  
*  Build scripts and CI/CD configuration  
*  Comprehensive test suite (unit \+ integration)  
*  Cross-platform installers/packages  
*  Migration guide from original app

Documentation Deliverables:

*  README with quick start guide  
*  Architecture documentation  
*  Developer setup guide  
*  User manual with screenshots  
*  API documentation (if applicable)  
*  Troubleshooting guide  
*  Changelog

Quality Deliverables:

*  Test coverage report (\>80%)  
*  Performance benchmark results  
*  Security audit report  
*  Dependency audit log  
*  Architecture Decision Records (ADRs)

---

SUCCESS CRITERIA  
The enhanced application is considered complete when:

1. ✅ Functionality: All original features work \+ all P0/P1 enhancements implemented  
2. ✅ Quality: Test coverage \>80%, no critical bugs, performance benchmarks met  
3. ✅ User Experience: First-time user can download video in \<60 seconds  
4. ✅ Cross-Platform: Works on Windows, macOS, Linux without platform-specific bugs  
5. ✅ Production Ready: Can be deployed, has update mechanism, handles errors gracefully  
6. ✅ Documentation: Complete user and developer documentation  
7. ✅ Free Tools: 100% open-source stack unless user approved paid alternatives  
8. ✅ Better Than Original: Measurably superior in UX, features, and performance

---

EXECUTION SEQUENCE

1. Week 1: Understand \+ Analyze (Phases 1-2)  
2. Week 2-4: Core Implementation (Phase 3, P0 features)  
3. Week 5-6: Enhanced Features (Phase 3, P1 features)  
4. Week 7: Verification (Phase 4\)  
5. Week 8: Refinement \+ Documentation (Phase 5\)  
6. Ongoing: Meta-recursive improvements based on learnings

---

FINAL INSTRUCTIONS  
Execute this project using the Cognitive Solver Architecture:

1. Don't skip the Understanding phase \- thorough analysis prevents rework  
2. Verify after every significant change \- catch issues early  
3. Use the meta-recursive protocol \- learn from each iteration  
4. Ask user before any paid solutions or major architectural changes  
5. Maintain quality gates \- don't compromise on tests or documentation  
6. Think in components \- build iteratively, verify continuously  
7. Document decisions \- future maintainers (including you) will thank you

Remember: Production-ready means it must work reliably for real users in real scenarios. Test thoroughly, handle errors gracefully, and provide excellent user experience.  
---

BEGIN EXECUTION WITH PHASE 1: UNDERSTAND  
