<template>
  <details class="er-guidance">
    <summary>How to categorise requests</summary>
    <div class="er-guidance-grid">
      <section>
        <h3>Priority</h3>
        <p><strong>P1:</strong> Engineering blocked, system unavailable, incorrect validation, risky export/manufacturing data, or blocked design workflow.</p>
        <p><strong>P2:</strong> Important operational issue, major slowdown, repeated workaround, significant support burden, or productivity impact with a workaround.</p>
        <p><strong>P3:</strong> Planned improvement, normal feature, report improvement, workflow enhancement, useful automation, or non-urgent change.</p>
        <p><strong>P4:</strong> Nice-to-have, cosmetic, convenience, low-impact idea, or optional improvement.</p>
      </section>
      <section>
        <h3>Status</h3>
        <p><strong>Incoming:</strong> New request not reviewed yet.</p>
        <p><strong>Planned:</strong> Reviewed and accepted, not currently being worked on.</p>
        <p><strong>InProgress:</strong> Actively being worked on.</p>
        <p><strong>Waiting:</strong> Blocked by clarification, test data, confirmation, deployment window, or external input.</p>
        <p><strong>Done:</strong> Completed, verified, and no further action needed.</p>
      </section>
      <section>
        <h3>Type</h3>
        <p><strong>Bug:</strong> Broken or incorrect behaviour.</p>
        <p><strong>Feature:</strong> New capability or enhancement.</p>
        <p><strong>Support:</strong> Help, investigation, user issue, or operational support.</p>
        <p><strong>Validation:</strong> Engineering rule, data check, verification logic, or validation behaviour.</p>
        <p><strong>Investigation:</strong> Unknown cause requiring analysis.</p>
        <p><strong>TechnicalDebt:</strong> Code, structure, reliability, testing, maintainability, or performance improvement.</p>
      </section>
    </div>
  </details>
</template>

<style scoped>
.er-guidance {
  border: 1px solid #d9dee5;
  border-radius: 6px;
  background: #f8fafc;
  padding: 0.75rem;
}

summary {
  cursor: pointer;
  font-weight: 700;
}

.er-guidance-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 1rem;
  margin-top: 0.75rem;
}

h3,
p {
  margin: 0;
}

h3 {
  font-size: 0.95rem;
  margin-bottom: 0.5rem;
}

p {
  color: #3f4a56;
  line-height: 1.35;
  margin-bottom: 0.45rem;
}
</style>
