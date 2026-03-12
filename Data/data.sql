-- Suppression préalable pour permettre la mise à jour
DROP VIEW IF EXISTS V_candidates;
DROP VIEW IF EXISTS V_results;
DROP VIEW IF EXISTS V_total_voters;
DROP VIEW IF EXISTS V_participation_rate;
DROP VIEW IF EXISTS V_dashboard;

-- Liste des candidats
CREATE VIEW V_candidates AS
SELECT 
    c.Id, c.NomCandidat, c.PrenomCandidat, c.Programme, c.Photo, c.Validated,
    f.NomFokontany AS fokontany, e.TitreElection AS election
FROM Candidats c
JOIN Fokontany f ON c.FokontanyId = f.Id
JOIN Elections e ON c.ElectionId = e.Id;

-- Résultats par candidat
CREATE VIEW V_results AS
SELECT 
    c.Id, c.NomCandidat, c.PrenomCandidat, 
    COUNT(v.Id) AS total_votes
FROM Candidats c
LEFT JOIN Votes v ON c.Id = v.CandidatId
GROUP BY c.Id;

-- Nombre de votants
CREATE VIEW V_total_voters AS
SELECT 
    e.Id AS election_id, 
    COUNT(v.Id) AS total_voters
FROM Elections e
LEFT JOIN Votes v ON e.Id = v.ElectionId
GROUP BY e.Id;

-- Taux de participation
CREATE VIEW V_participation_rate AS
SELECT 
    e.Id, e.TitreElection,
    IF(COUNT(c.Id) = 0, 0, (COUNT(v.Id) / COUNT(c.Id)) * 100) AS participation_rate
FROM Elections e
LEFT JOIN Citoyens c ON c.FokontanyId = e.FokontanyId
LEFT JOIN Votes v ON v.ElectionId = e.Id
GROUP BY e.Id;

-- Dashboard admin
CREATE VIEW V_dashboard AS
SELECT 
    (SELECT COUNT(*) FROM Citoyens) AS total_citoyens,
    (SELECT COUNT(*) FROM CinRequests WHERE status='APPROVED') AS cin_delivered,
    (SELECT COUNT(*) FROM CinRequests WHERE status='PENDING') AS pending_requests,
    (SELECT COUNT(*) FROM Votes) AS total_votes;